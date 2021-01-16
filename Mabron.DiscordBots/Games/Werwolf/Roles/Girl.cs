using System;
using System.Collections.Generic;
using System.Text;

namespace Mabron.DiscordBots.Games.Werwolf.Roles
{
    public class Girl : VillagerBase
    {
        public override string Name => "Mädchen";

        public override string Description => "Darf in der Nacht \"blinzeln\" um zu erkennen wer die Werwölfe sind.";

        public override Role CreateNew()
        {
            return new Girl();
        }
    }
}
