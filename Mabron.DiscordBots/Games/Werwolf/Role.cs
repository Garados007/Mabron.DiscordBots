namespace Mabron.DiscordBots.Games.Werwolf
{
    /// <summary>
    /// The basic role every game user has. Any special state is encoded in this role.
    /// </summary>
    public abstract class Role
    {
        public bool IsAlive { get; set; }

        public bool IsMajor { get; set; }

        public bool IsSelectedByWerewolves { get; set; } = false;

        public virtual void Reset()
        {
            IsAlive = true;
            IsMajor = false;
        }

        public Role()
        {
            Reset();
        }

        public abstract bool? IsSameFaction(Role other);

        public virtual Role ViewRole(Role viewer)
        {
            return new Roles.Villager();
        }

        public abstract Role CreateNew();

        public abstract string Name { get; }

        public abstract string Description { get; }
    }
}