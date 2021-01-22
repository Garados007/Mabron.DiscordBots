namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Idiot : VillagerBase
    {
        public bool IsRevealed { get; set; }

        public bool WasMajor { get; set; }

        public Idiot(Theme theme) : base(theme)
        {
        }

        public override Role ViewRole(Role viewer)
        {
            if (IsRevealed)
                return this;
            return base.ViewRole(viewer);
        }

        public override string Name => "Dorfdepp";

        public override string Description => "Ein Dorfdepp verliert sein Stimmrecht, wenn er vom Dorf getöted werden soll";

        public override Role CreateNew()
        {
            return new Idiot(Theme);
        }
    }
}
