namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Witch : VillagerBase
    {
        public bool UsedLivePotion { get; set; }

        public bool UsedDeathPotion { get; set; }

        public Witch(Theme theme) : base(theme)
        {
        }

        public override void Reset()
        {
            base.Reset();
            UsedDeathPotion = false;
            UsedLivePotion = false;
        }

        public override string Name => "Hexe";

        public override string Description => "Die Hexe hat zwei Tränke. Mit einen kann sie " +
            "einmal das Opfer der Werwölfe retten und mit den anderen kann sie jemand anderen " +
            "einmalig vergiften.";

        public override Role CreateNew()
        {
            return new Witch(Theme);
        }

        public override Role ViewRole(Role viewer)
        {
            if (viewer is Witch)
                return this;
            return base.ViewRole(viewer);
        }
    }
}
