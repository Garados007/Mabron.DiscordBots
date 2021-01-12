namespace Mabron.DiscordBots.Games.Werwolf.Roles
{
    public abstract class VillagerBase : Role
    {
        public override bool? IsSameFaction(Role other)
        {
            if (other is VillagerBase)
                return true;
            return null;
        }
    }
}
