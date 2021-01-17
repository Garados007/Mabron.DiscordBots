namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Amor : VillagerBase
    {
        public Amor(Theme theme) : base(theme)
        {
        }

        public override string Name => "Amor";

        public override string Description => "Amor verbindet zwei Liebenden fürs Leben";

        public override Role CreateNew()
        {
            return new Amor(Theme);
        }
    }
}
