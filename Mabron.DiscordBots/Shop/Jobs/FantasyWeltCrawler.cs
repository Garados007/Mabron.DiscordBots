using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Shop.Jobs
{
    public class FantasyWeltCrawler : CrawlerBase
    {
        readonly Regex entryRegex = new Regex(@"(?:(?<img>https://www.fantasywelt.de/media/image/product/[^""]+)(?:[^<]|<[^h]|<h[^4])*)<h4\sclass=\""title\""><[^>]+>(?<name>[^<]*)<.*?<div\sclass=\""price_wrapper\"".*?<strong\sclass=\""price\s[^>]+>\s*<span>(?<price>\d+(?:,\d+)?)[^\d]",
            RegexOptions.Singleline | RegexOptions.Compiled);
        readonly Regex uvpRegex = new Regex(@"<div class=\""instead-of[^>]+>[^\d]*(?<uvp>\d+(?:,\d+)?) ",
            RegexOptions.Singleline | RegexOptions.Compiled);
        readonly Regex nextPage = new Regex(@"<ul\sclass=\""pagination.*?<li class=""next"">\s*<a href=\""(?<url>[^\""]+)\""",
            RegexOptions.Singleline | RegexOptions.Compiled);
        readonly Regex getUrl = new Regex(@"<a href=\""(?<url>[^\""]*)\""",
            RegexOptions.Singleline | RegexOptions.Compiled);

        public override async Task<(IEnumerable<CrawlerResult> result, Uri? next)> Crawl(Uri site)
        {
            if (site.Scheme.ToLower() != "http" && site.Scheme.ToLower() != "https")
                throw new NotSupportedException($"Protocol {site.Scheme} not supported");
            if (site.DnsSafeHost.ToLower() != "www.fantasywelt.de")
                throw new NotSupportedException($"Domain {site.DnsSafeHost} not supported");
            using var client = new WebClient()
            {
                Encoding = Encoding.GetEncoding("iso-8859-1"),
            };
            var page = await client.DownloadStringTaskAsync(site);
            var next = nextPage.Match(page);
            return (GetResults(site.AbsoluteUri, page),
                next != null && next.Success ? new Uri(next.Groups["url"].Value) : null);
        }

        private IEnumerable<CrawlerResult> GetResults(string site, string content)
        {
            foreach (Match? result in entryRegex.Matches(content))
            {
                if (!result!.Success)
                    continue;
                Match match;
                if ((match = getUrl.Match(result.Value)) == null || !match.Success)
                    continue;
                var img = result.Groups["img"].Value;
                var url = match.Groups["url"].Value;
                var name = result.Groups["name"].Value;
                var price = float.Parse(result.Groups["price"].Value.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                float retail = price;
                if ((match = uvpRegex.Match(result.Value)) != null && match.Success)
                {
                    retail = float.Parse(match.Groups["uvp"].Value.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                }
                yield return new CrawlerResult
                {
                    Date = DateTime.UtcNow,
                    Name = name,
                    ShopPrice = price,
                    SourceWebsite = new Uri(new Uri(site), url).AbsoluteUri,
                    RetailPrice = retail,
                    ItemImage = img,
                };
            }
        }
    }
}
