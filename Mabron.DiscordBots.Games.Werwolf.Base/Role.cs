using System;

namespace Mabron.DiscordBots.Games.Werwolf
{
    /// <summary>
    /// The basic role every game user has. Any special state is encoded in this role.
    /// </summary>
    public abstract class Role
    {
        public bool IsAlive { get; set; }

        public bool IsMajor { get; set; }


        public virtual void Reset()
        {
            IsAlive = true;
            IsMajor = false;
        }

        public Theme Theme { get; }

        public Role(Theme theme)
        {
            Theme = theme ?? throw new ArgumentNullException(nameof(theme));
            Reset();
        }

        public abstract bool? IsSameFaction(Role other);

        public virtual Role ViewRole(Role viewer)
        {
            return Theme.GetBasicRole();
        }

        public virtual bool ViewLoved(Role viewer)
        {
            return false;
        }

        public abstract Role CreateNew();

        public abstract string Name { get; }

        public abstract string Description { get; }
    }
}