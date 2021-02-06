using System;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public class CheckWinConditionAction : ActionPhaseBase
    {
        public override void Execute(GameRoom game)
        {
            if (new WinCondition().Check(game, out ReadOnlyMemory<Role>? winner))
            {
                game.StopGame(winner);
            }
        }
    }
}
