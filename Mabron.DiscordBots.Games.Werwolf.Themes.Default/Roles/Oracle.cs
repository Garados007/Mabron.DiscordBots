namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Oracle : VillagerBase
    {
        public Oracle(Theme theme) : base(theme)
        {
        }

        public override string Name => "alte Seherin";

        public override string Description => "Ihr ist es erlaubt die wahre Indentität eines Spieler zu entdecken.";

        public override Role CreateNew()
        {
            return new Oracle(Theme);
        }

        public override Role ViewRole(Role viewer)
        {
            if (viewer is Oracle)
                return this;
            return base.ViewRole(viewer);
        }
    }
}
