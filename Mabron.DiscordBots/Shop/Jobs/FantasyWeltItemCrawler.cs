using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Shop.Jobs
{
    public class FantasyWeltItemCrawler : CrawlerBase
    {
        readonly Regex titleRegex = new Regex(@"<h1 class=""fn product-title"" itemprop=""name"">(?<name>[^<]*)<",
            RegexOptions.Singleline | RegexOptions.Compiled);
        readonly Regex priceRegex = new Regex(@"<div class=""outer_price_wrap"">\s+<div class=""price_wrapper"".*<meta itemprop=""price"" content=""(?<price>\d+(\.\d+)?)""",
            RegexOptions.Singleline | RegexOptions.Compiled);
        readonly Regex uvpRegex = new Regex(@"<div class=\""instead-of[^>]+>[^\d]*(?<uvp>\d+(?:,\d+)?) ",
            RegexOptions.Singleline | RegexOptions.Compiled);
        readonly Regex imgRegex = new Regex(@"media/image/product/\d+/lg/[^""]+ ",
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
            return (GetResults(site.AbsoluteUri, page), null);
        }

        private IEnumerable<CrawlerResult> GetResults(string site, string content)
        {
            Match result = titleRegex.Match(content);
            if (!result.Success)
                yield break;
            var name = result.Groups["name"].Value;
            result = priceRegex.Match(content);
            if (!result.Success)
                yield break;
            var price = float.Parse(result.Groups["price"].Value.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
            float retail = price;
            if ((result = uvpRegex.Match(result.Value)) != null && result.Success)
            {
                retail = float.Parse(result.Groups["uvp"].Value.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
            }
            result = imgRegex.Match(content);
            var img = result.Success ? result.Value : null;
            yield return new CrawlerResult
            {
                Date = DateTime.UtcNow,
                Name = name,
                ShopPrice = price,
                SourceWebsite = site,
                RetailPrice = retail,
                ItemImage = img == null ? null : $"https://www.fantasywelt.de/{img}"
            };
        }
    }
}
