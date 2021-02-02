using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class RemoveVoting : GameEvent
    {
        public ulong Id { get; }

        public RemoveVoting(ulong id)
            => Id = id;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteString("id", Id.ToString());
        }
    }
}
