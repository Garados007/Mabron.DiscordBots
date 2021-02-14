using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class SetUserConfig : GameEvent
    {
        public GameUser User { get; }

        public SetUserConfig(GameUser user)
            => User = user;

        public override bool CanSendTo(GameRoom game, GameUser user)
        {
            return user.Id == User.Id;
        }

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            var userConfig = Theme.User!.Query()
                .Where(x => x.Id == user.Id)
                .FirstOrDefault();
            if (userConfig != null)
            {
                writer.WriteStartObject("user-config");
                writer.WriteString("theme", userConfig.ThemeColor ?? "#ffffff");
                writer.WriteString("background", userConfig.BackgroundImage ?? "");
                writer.WriteString("language", string.IsNullOrEmpty(userConfig.Language) ? "de" : userConfig.Language);
                writer.WriteEndObject();
            }
            else writer.WriteNull("user-config");
        }
    }
}
