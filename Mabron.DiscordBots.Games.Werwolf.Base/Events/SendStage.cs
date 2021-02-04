using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class SendStage : GameEvent
    {
        public Stage Stage { get; }

        public SendStage(Stage stage)
            => Stage = stage;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteString("lang-id", Stage.LanguageId);
            writer.WriteString("background-id", Stage.BackgroundId);
        }
    }
}
