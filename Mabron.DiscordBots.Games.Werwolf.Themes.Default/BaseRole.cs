using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default
{
    public abstract class BaseRole : Role
    {
        protected BaseRole(Theme theme) : base(theme)
        {
        }

        private bool isSelectedByWerewolves = false;
        public bool IsSelectedByWerewolves
        {
            get => isSelectedByWerewolves;
            set
            {
                isSelectedByWerewolves = value;
                SendRoleInfoChanged();
            }
        }

        public bool IsSelectedByHealer { get; set; } = false;

        private bool isViewedByOracle = false;
        public bool IsViewedByOracle
        {
            get => isViewedByOracle;
            set
            {
                isViewedByOracle = value;
                SendRoleInfoChanged();
            }
        }

        private bool isLoved = false;
        public bool IsLoved
        {
            get => isLoved;
            set
            {
                isLoved = value;
                SendRoleInfoChanged();
            }
        }

        public bool HasVotePermitFromScapeGoat { get; set; } = false;

        private bool isEnchantedByFlutist = false;
        public bool IsEnchantedByFlutist
        {
            get => isEnchantedByFlutist;
            set
            {
                isEnchantedByFlutist = value;
                SendRoleInfoChanged();
            }
        }

        public override IEnumerable<string> GetTags(GameRoom game, Role? viewer)
        {
            foreach (var tag in base.GetTags(game, viewer))
                yield return tag;
            if (IsLoved && (viewer == this || viewer == null || ViewLoved(viewer)))
                yield return "loved";
            if (IsSelectedByWerewolves && (viewer == null || viewer is Roles.Witch))
                yield return "werwolf-select";
            if (IsEnchantedByFlutist && (viewer == null || viewer is Roles.Flutist || (viewer is BaseRole baseRole && baseRole.IsEnchantedByFlutist)))
                yield return "enchant-flutist";
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
