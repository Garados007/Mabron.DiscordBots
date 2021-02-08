using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Shop.Jobs
{
    public abstract class CrawlerBase
    {
        public abstract Task<(IEnumerable<CrawlerResult> result, Uri? next)> Crawl(Uri site);

        public async IAsyncEnumerable<CrawlerResult> CrawlAll(Uri site)
        {
            _ = site ?? throw new ArgumentNullException(nameof(site));
            var next = site;
            while (next != null)
            {
                (IEnumerable<CrawlerResult> result, Uri? next) result;
                try { result = await Crawl(next); }
                catch (Exception e)
                {
                    await Console.Out.WriteLineAsync($"[{DateTime.Now:G}] [Error] [Crawl] Error with uri {site}: {e}");
                    break;
                }
                foreach (var entry in result.result)
                    yield return entry;
                next = result.next;
            }
        }

        public async Task<CrawlerResult?> CrawlSingle(Uri site)
        {
            var result = await Crawl(site);
            var list = result.result.ToArray();
            return list.Length == 0 ? null : list[0];
        }
    }
}
