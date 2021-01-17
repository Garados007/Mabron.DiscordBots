namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Hunter : VillagerBase
    {
        public Hunter(Theme theme) : base(theme)
        {
        }

        public bool HasKilled { get; set; } = false;

        public override string Name => "Jäger";

        public override string Description => "Zieht bei seinen Tod einen beliebigen anderen Mitspieler auch in den Tod.";

        public override Role CreateNew()
        {
            return new Hunter(Theme);
        }

        public override Role ViewRole(Role viewer)
        {
            if (viewer is Hunter)
                return this;
            return base.ViewRole(viewer);
        }

        public override void Reset()
        {
            base.Reset();
            HasKilled = false;
        }
    }
}
