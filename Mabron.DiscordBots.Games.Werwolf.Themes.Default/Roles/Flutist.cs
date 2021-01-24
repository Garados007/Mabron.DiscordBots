﻿namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Flutist : BaseRole
    {
        public Flutist(Theme theme) 
            : base(theme)
        {
        }

        public override string Name => "Der Flötenspieler";

        public override string Description => "Der Flötenspieler verzaubert das ganze Dorf zu seinen gunsten";

        public override Role CreateNew()
            => new Flutist(Theme);

        public override bool? IsSameFaction(Role other)
        {
            return other is Flutist;
        }
    }
}