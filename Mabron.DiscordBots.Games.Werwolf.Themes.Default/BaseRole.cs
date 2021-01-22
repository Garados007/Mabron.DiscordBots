using System;
using System.Collections.Generic;
using System.Text;

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

        public bool HasSeenLoved { get; set; } = false;

        public override void Reset()
        {
            base.Reset();
            IsSelectedByWerewolves = false;
            IsSelectedByHealer = false;
            IsViewedByOracle = false;
            IsLoved = false;
            HasSeenLoved = false;
        }

        public override Role ViewRole(Role viewer)
        {
            if (IsViewedByOracle && viewer is Roles.Oracle)
                return this;
            return base.ViewRole(viewer);
        }

        public override bool ViewLoved(Role viewer)
        {
            if (!(viewer is BaseRole viewer_))
                return base.ViewLoved(viewer);
            if (viewer_.IsLoved || viewer is Roles.Amor)
                return IsLoved;
            return base.ViewLoved(viewer);
        }
    }
}
