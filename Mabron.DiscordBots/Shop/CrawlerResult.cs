using LiteDB;
using System;

namespace Mabron.DiscordBots.Shop
{
    public class CrawlerResult
    {
        public ObjectId Id { get; set; } = new ObjectId();

        public string SourceWebsite { get; set; } = "";

        public string Name { get; set; } = "";

        public float RetailPrice { get; set; }

        public float ShopPrice { get; set; }

        public string? ItemImage { get; set; }

        public DateTime Date { get; set; }

        public override string ToString()
            => $"[{Date}] {Name}: {RetailPrice} -> {ShopPrice}";
    }
}
