namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default
{
    public abstract class WerwolfBase : BaseRole
    {
        protected WerwolfBase(Theme theme) : base(theme)
        {
        }

        public override bool? IsSameFaction(Role other)      
        {
            if (other is WerwolfBase)
                return true;
            return null;
        }

        public override Role ViewRole(Role viewer)
        {
            if (viewer is WerwolfBase || viewer is Roles.Girl)
                return new Roles.Werwolf(Theme);
            return base.ViewRole(viewer);
        }
    }
}
