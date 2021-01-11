using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mabron.DiscordBots.Shop
{
    public class SentToast
    {
        public ObjectId Id { get; set; } = new ObjectId();

        public ulong RoomId { get; set; }

        public ulong MessageId { get; set; }

        [BsonRef("shop_item")]
        public CrawlerResult? ShopEntry { get; set; }

        public DateTime SentDate { get; set; }
    }
}
