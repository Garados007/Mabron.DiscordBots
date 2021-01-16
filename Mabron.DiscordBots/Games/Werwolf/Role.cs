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

        public bool IsViewedByOracle { get; set; } = false;

        public bool IsLoved { get; set; } = false;

        public bool HasSeenLoved { get; set; } = false;

        public virtual void Reset()
        {
            IsAlive = true;
            IsMajor = false;
            IsSelectedByWerewolves = false;
            IsViewedByOracle = false;
            IsLoved = false;
            HasSeenLoved = false;
        }

        public Role()
        {
            Reset();
        }

        public abstract bool? IsSameFaction(Role other);

        public virtual Role ViewRole(Role viewer)
        {
            if (IsViewedByOracle && viewer is Roles.Oracle)
                return this;
            return new Roles.Villager();
        }

        public virtual bool ViewLoved(Role viewer)
        {
            if (viewer.IsLoved || viewer is Roles.Amor)
                return IsLoved;
            return false;
        }

        public abstract Role CreateNew();

        public abstract string Name { get; }

        public abstract string Description { get; }
    }
}