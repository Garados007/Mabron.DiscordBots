using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default
{
    public abstract class BaseRole : Role
    {
        protected BaseRole(Theme theme) : base(theme)
        {
        }

        public bool IsSelectedByWerewolves { get; set; } = false;

        public bool IsSelectedByHealer { get; set; } = false;

        public bool IsViewedByOracle { get; set; } = false;

        public bool IsLoved { get; set; } = false;

        public bool HasVotePermitFromScapeGoat { get; set; } = false;

        public override IEnumerable<string> GetTags(GameRoom game, Role? viewer)
        {
            foreach (var tag in base.GetTags(game, viewer))
                yield return tag;
            if (IsLoved && (viewer == this || viewer == null || ViewLoved(viewer)))
                yield return "loved";
            if (IsSelectedByWerewolves && (viewer == null || viewer is Roles.Witch))
                yield return "werwolf-select";
        }

        public override void Reset()
        {
            base.Reset();
            IsSelectedByWerewolves = false;
            IsSelectedByHealer = false;
            IsViewedByOracle = false;
            IsLoved = false;
            HasVotePermitFromScapeGoat = false;
        }

        public override Role ViewRole(Role viewer)
        {
            if (IsViewedByOracle && viewer is Roles.Oracle)
                return this;
            return base.ViewRole(viewer);
        }

        public virtual bool ViewLoved(Role viewer)
        {
            if (!(viewer is BaseRole viewer_))
                return false;
            if (viewer_.IsLoved || viewer is Roles.Amor)
                return IsLoved;
            return false;
        }

        public override void Kill(GameRoom game)
            => Kill(game, true);

        private void Kill(GameRoom game, bool checkLoved)
        {
            base.Kill(game);
            if (IsLoved && checkLoved)
                foreach (var role in game.AliveRoles)
                    if (role is BaseRole baseRole && baseRole.IsLoved)
                        baseRole.Kill(game, false);
        }
    }
}
