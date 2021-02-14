namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class ThreeBrothers : VillagerBase
    {
        public ThreeBrothers(Theme theme) : base(theme)
        {
        }

        public override string Name => "ThreeBrothers";

        public override Role CreateNew()
            => new ThreeBrothers(Theme);
    }
}
