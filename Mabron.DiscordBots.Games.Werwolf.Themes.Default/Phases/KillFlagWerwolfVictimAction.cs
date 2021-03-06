﻿using Mabron.DiscordBots.Games.Werwolf.Phases;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class KillFlagWerwolfVictimAction : ActionPhaseBase
    {
        public override void Execute(GameRoom game)
        {
            foreach (var role in game.Participants.Values)
            {
                if (!(role?.KillInfo is KillInfos.KilledByWerwolf) || !(role is BaseRole baseRole))
                    continue;
                if (baseRole.IsSelectedByHealer)
                {
                    role.RemoveKillFlag();
                    continue;
                }
                if (baseRole is Roles.OldMan oldMan && !oldMan.WasKilledByWolvesOneTime)
                {
                    oldMan.WasKilledByWolvesOneTime = true;
                    role.RemoveKillFlag();
                    continue;
                }
            }
        }
    }
}
