using System;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf
{
    /// <summary>
    /// The basic role every game user has. Any special state is encoded in this role.
    /// </summary>
    public abstract class Role
    {
        public bool IsAlive { get; set; }

        public bool IsMajor { get; set; }

        /// <summary>
        /// Get a list of special tags that are defined for this role. 
        /// </summary>
        /// <param name="game">The current game</param>
        /// <param name="viewer">The viewer of this role. null for the leader</param>
        /// <returns>a list of defined tags</returns>
        public virtual IEnumerable<string> GetTags(GameRoom game, Role? viewer)
        {
            if (IsAlive)
                yield return "alive";
            if (IsMajor)
                yield return "major";
        }

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

        public abstract Role CreateNew();

        public abstract string Name { get; }

        public abstract string Description { get; }
    }
}