using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.KillInfos
{
    public class KilledByHunter : KillInfo
    {
        public override string NotificationId => "hunter-kill";

        public override IEnumerable<string> GetKillFlags(GameRoom game, Role? viewer)
        {
            return Enumerable.Empty<string>();
        }
    }
}
