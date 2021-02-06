using LiteDB;
using System;
using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class PlayerNotification : GameEvent
    {
        public string NotificationId { get; }

        public ReadOnlyMemory<ObjectId> Player { get; }

        public PlayerNotification(string notificationId, ReadOnlyMemory<ObjectId> player)
            => (NotificationId, Player) = (notificationId, player);

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteString("text-id", NotificationId);
            writer.WriteStartArray("player");
            foreach (var id in Player.Span)
                writer.WriteStringValue(id.ToString());
            writer.WriteEndArray();
        }
    }
}
