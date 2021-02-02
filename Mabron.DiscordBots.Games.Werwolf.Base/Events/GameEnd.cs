using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class GameEnd : GameEvent
    {
        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            if (game.Winner == null)
                writer.WriteNull("winner");
            else
            {
                writer.WriteStartArray("winner");
                foreach (var id in game.Winner.Value.winner.Span)
                    writer.WriteStringValue(id.ToString());
                writer.WriteEndArray();
            }
        }
    }
}
