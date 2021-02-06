using LiteDB;
using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class RemoveParticipant : GameEvent
    {
        public ObjectId UserId { get; }

        public RemoveParticipant(ObjectId userId)
            => UserId = userId;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteString("id", UserId.ToString());
        }
    }
}