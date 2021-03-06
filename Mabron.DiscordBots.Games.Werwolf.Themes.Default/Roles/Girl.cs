﻿using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class Girl : VillagerBase
    {
        private readonly List<WerwolfBase> seenByWolf
            = new List<WerwolfBase>();
        private readonly object lockSeenByWolf = new object();

        public void AddSeenByWolf(WerwolfBase wolf)
        {
            lock (lockSeenByWolf)
                seenByWolf.Add(wolf);
            SendRoleInfoChanged();
        }

        public bool IsSeenByWolf(WerwolfBase wolf)
        {
            lock (lockSeenByWolf)
                return seenByWolf.Contains(wolf);
        }

        public Girl(Theme theme) : base(theme)
        {
        }

        public override string Name => "Mädchen";

        public override Role CreateNew()
        {
            return new Girl(Theme);
        }

        public override Role ViewRole(Role viewer)
        {
            if (viewer is WerwolfBase wolf && IsSeenByWolf(wolf))
                return this;
            return base.ViewRole(viewer);
        }
    }
}
