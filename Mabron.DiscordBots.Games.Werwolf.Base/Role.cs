using System;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf
{
    /// <summary>
    /// The basic role every game user has. Any special state is encoded in this role.
    /// </summary>
    public abstract class Role
    {
        private bool isAlive = true;
        public bool IsAlive
        {
            get => isAlive;
            set
            {
                isAlive = value;
                SendRoleInfoChanged();
            }
        }

        private bool isMajor = false;
        public bool IsMajor
        {
            get => isMajor;
            set
            {
                isMajor = value;
                SendRoleInfoChanged();
            }
        }

        /// <summary>
        /// Get a list of special tags that are defined for this role. 
        /// </summary>
        /// <param name="game">The current game</param>
        /// <param name="viewer">The viewer of this role. null for the leader</param>
        /// <returns>a list of defined tags</returns>
        public virtual IEnumerable<string> GetTags(GameRoom game, Role? viewer)
        {
            if (!IsAlive)
                yield return "not-alive";
            if (IsMajor)
                yield return "major";
        }

        public void SendRoleInfoChanged()
        {
            Theme.Game?.SendEvent(new Events.OnRoleInfoChanged(this));
        }

        public Theme Theme { get; }

        public Role(Theme theme)
        {
            Theme = theme ?? throw new ArgumentNullException(nameof(theme));
        }

        public abstract bool? IsSameFaction(Role other);

        public virtual Role ViewRole(Role viewer)
        {
            return Theme.GetBasicRole();
        }

        public abstract Role CreateNew();

        public abstract string Name { get; }

        public virtual void Kill(GameRoom game)
        {
            IsAlive = false;
        }

        public static Role? GetSeenRole(GameRoom game, uint? round, GameUser user, ulong targetId, Role target)
        {
            var ownRole = game.TryGetRole(user.DiscordId);
            return (game.Leader == user.DiscordId && !game.LeaderIsPlayer) ||
                    targetId == user.DiscordId ||
                    round == game.ExecutionRound ||
                    (ownRole != null && game.DeadCanSeeAllRoles && !ownRole.IsAlive) ?
                target :
                ownRole != null ?
                target.ViewRole(ownRole) :
                null;
        }
    }
}