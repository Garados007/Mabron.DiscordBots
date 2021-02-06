using System;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public class KillTransitionToKilledAction : ActionPhaseBase
    {
        public override void Execute(GameRoom game)
        {
            foreach (var (id, role) in game.Participants)
                if (role != null && role.KillState == KillState.BeforeKill)
                {
                    role.ChangeToKilled();
                    if (game.DeadCanSeeAllRoles)
                        foreach (var otherRole in game.Participants.Values)
                            if (otherRole != null)
                                game.SendEvent(new Events.OnRoleInfoChanged(otherRole, target: id));
                }
            if (new WinCondition().Check(game, out ReadOnlyMemory<Role>? winner))
            {
                game.StopGame(winner);
            }
        }
    }
}
