using LiteDB;
using System.Collections.Generic;
using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class MultiPlayerNotification : GameEvent
    {
        public Dictionary<string, HashSet<ObjectId>> Notifications { get; }

        public MultiPlayerNotification(Dictionary<string, HashSet<ObjectId>> notifications)
            => Notifications = notifications;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteStartObject("notifications");
            foreach (var (key, players) in Notifications)
            {
                writer.WriteStartArray(key);
                foreach (var player in players)
                    writer.WriteStringValue(player.ToString());
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }
}
