using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public abstract class GameEvent
    {
        public abstract bool CanSendTo(GameRoom game, GameUser user);

        public virtual string GameEventType
            => GetType().Name;
        
        public void Write(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteStartObject();
            writer.WriteString("$type", GameEventType);
            WriteContent(writer, game, user);
            writer.WriteEndObject();
        }

        public abstract void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user);
    }
}