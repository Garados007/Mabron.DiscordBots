using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.KillInfos
{
    public class OldManKillsIdiot : KillInfo
    {
        public override string NotificationId => "old-man-and-idiot-killed";

        public override IEnumerable<string> GetKillFlags(GameRoom game, Role? viewer)
        {
            return Enumerable.Empty<string>();
        }
    }
}
