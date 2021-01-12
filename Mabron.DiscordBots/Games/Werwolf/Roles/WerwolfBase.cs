namespace Mabron.DiscordBots.Games.Werwolf.Roles
{
    public abstract class WerwolfBase : Role
    {
        public override bool? IsSameFaction(Role other)
        {
            if (other is WerwolfBase)
                return true;
            return null;
        }

        public override Role ViewRole(Role viewer)
        {
            if (viewer is WerwolfBase)
                return new Werwolf();
            return base.ViewRole(viewer);
        }
    }
}
