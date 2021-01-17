namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default
{
    public abstract class VillagerBase : BaseRole
    {
        protected VillagerBase(Theme theme) : base(theme)
        {
        }

        public override bool? IsSameFaction(Role other)
        {
            if (other is VillagerBase)
                return true;
            return null;
        }
    }
}
