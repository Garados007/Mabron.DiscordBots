using Mabron.DiscordBots.Games.Werwolf.Phases;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class KillWerwolfVictimAction : ActionPhaseBase
    {
        public override void Execute(GameRoom game)
        {
            foreach (var role in game.Participants.Values)
            {
                if (!(role is BaseRole baseRole))
                    continue;
                if (baseRole.IsSelectedByWerewolves)
                {
                    baseRole.IsSelectedByWerewolves = false;
                    if (baseRole.IsSelectedByHealer)
                        continue;
                    if (baseRole is Roles.OldMan oldMan && !oldMan.WasKilledByWolvesOneTime)
                    {
                        oldMan.WasKilledByWolvesOneTime = true;
                        continue;
                    }
                    baseRole.Kill(game);
                }
            }
        }
    }
}
