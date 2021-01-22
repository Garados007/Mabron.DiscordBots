using Mabron.DiscordBots.Games.Werwolf.Phases;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class KillWerwolfVictim : ActionPhaseBase
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
                    baseRole.IsAlive = false;
                    if (baseRole.IsLoved)
                        foreach (var other in game.AliveRoles)
                            if (other is BaseRole otherBase && otherBase.IsLoved)
                                other.IsAlive = false;
                }
            }
        }
    }
}
