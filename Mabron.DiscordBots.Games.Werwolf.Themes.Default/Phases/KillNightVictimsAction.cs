using Mabron.DiscordBots.Games.Werwolf.Phases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class KillNightVictimsAction : ActionPhaseBase
    {
        public override void Execute(GameRoom game)
        {
            var killedOnes = new HashSet<ulong>();
            foreach (var (id, role) in game.Participants)
            {
                if (!(role is BaseRole baseRole))
                    continue;
                if (baseRole.IsAboutToBeKilled)
                {
                    killedOnes.Add(id);
                    baseRole.RealKill(game, null);
                }
            }
            if (killedOnes.Count > 0)
                game.SendEvent(new Events.PlayerNotification(
                    "night-kills",
                    killedOnes.ToArray()
                ));
            if (new WinCondition().Check(game, out ReadOnlyMemory<Role>? winner))
            {
                game.StopGame(winner);
            }
        }
    }
}
