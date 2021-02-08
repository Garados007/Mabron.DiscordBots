using LiteDB;
using System;

namespace Mabron.DiscordBots.Shop
{
    public class Database : IDisposable
    {
        private readonly LiteDatabase db;

        public ILiteCollection<ChatRoom> ChatRooms { get; }

        public ILiteCollection<CrawlerResult> Results { get; }

        public ILiteCollection<SentToast> SentToasts { get; }

        public Database()
        {
            db = new LiteDatabase("shops.litedb");
            ChatRooms = db.GetCollection<ChatRoom>("chat_room", BsonAutoId.Int64);
            Results = db.GetCollection<CrawlerResult>("result");
            SentToasts = db.GetCollection<SentToast>("send_toast");
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
