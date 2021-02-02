using System.Linq;
using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class NextPhase : GameEvent
    {
        public Phase? Phase { get; }

        public NextPhase(Phase? phase)
            => Phase = phase;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            if (Phase == null)
                writer.WriteNull("phase");
            else
            {
                writer.WriteStartObject("phase"); // phase
                writer.WriteString("lang-id", Phase.LanguageId);
                writer.WriteEndObject();
            }
        }
    }
}