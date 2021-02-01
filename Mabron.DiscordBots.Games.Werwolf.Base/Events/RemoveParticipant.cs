using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class RemoveParticipant : GameEvent
    {
        public ulong UserId { get; }

        public RemoveParticipant(ulong userId)
            => UserId = userId;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteString("id", UserId.ToString());
        }
    }
}