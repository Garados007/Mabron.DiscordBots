using System;
using System.Collections.Generic;
using System.Text;

namespace Mabron.DiscordBots.Games.Werwolf.Roles
{
    public class Hunter : VillagerBase
    {
        public bool HasKilled { get; set; } = false;

        public override string Name => "Jäger";

        public override string Description => "Zieht bei seinen Tod einen beliebigen anderen Mitspieler auch in den Tod.";

        public override Role CreateNew()
        {
            return new Hunter();
        }

        public override Role ViewRole(Role viewer)
        {
            if (viewer is Hunter)
                return this;
            return base.ViewRole(viewer);
        }

        public override void Reset()
        {
            base.Reset();
            HasKilled = false;
        }
    }
}
