﻿using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class SetUserConfig : GameEvent
    {
        public GameUser User { get; }

        public SetUserConfig(GameUser user)
            => User = user;

        public override bool CanSendTo(GameRoom game, GameUser user)
        {
            return user.DiscordId == User.DiscordId;
        }

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            var userConfig = Theme.User!.Query()
                .Where(x => x.DiscordId == user.DiscordId)
                .FirstOrDefault();
            if (userConfig != null)
            {
                writer.WriteStartObject("user-config");
                writer.WriteString("theme", userConfig.ThemeColor ?? "#ffffff");
                writer.WriteString("background", userConfig.BackgroundImage ?? "");
                writer.WriteEndObject();
            }
            else writer.WriteNull("user-config");
        }
    }
}