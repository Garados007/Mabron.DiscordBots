namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Girl : VillagerBase
    {
        public Girl(Theme theme) : base(theme)
        {
        }

        public override string Name => "Mädchen";

        public override Role CreateNew()
        {
            return new Girl(Theme);
        }
    }
}
