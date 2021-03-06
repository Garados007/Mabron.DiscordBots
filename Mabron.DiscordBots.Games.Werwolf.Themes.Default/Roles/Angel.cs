﻿namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Angel : VillagerBase
    {
        public bool MissedFirstRound { get; set; } = false;

        public Angel(Theme theme) : base(theme)
        {
        }

        public override string Name => "Angel";

        public override Role CreateNew()
        {
            return new Angel(Theme);
        }
    }
}
