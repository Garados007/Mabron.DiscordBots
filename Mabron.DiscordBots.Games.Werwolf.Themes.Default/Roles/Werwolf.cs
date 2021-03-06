﻿namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Werwolf : WerwolfBase
    {
        public Werwolf(Theme theme) : base(theme)
        {
        }

        public override string Name => "Werwolf";

        public override Role CreateNew()
        {
            return new Werwolf(Theme);
        }
    }
}
