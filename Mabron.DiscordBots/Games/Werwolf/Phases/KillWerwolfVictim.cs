namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public class KillWerwolfVictim : ActionPhaseBase
    {
        public override void Execute(GameRoom game)
        {
            foreach (var role in game.Participants.Values)
            {
                if (role == null)
                    continue;
                if (role.IsSelectedByWerewolves)
                {
                    role.IsAlive = false;
                    role.IsSelectedByWerewolves = false;
                    if (role.IsLoved)
                        foreach (var other in game.AliveRoles)
                            if (other.IsLoved)
                                other.IsAlive = false;
                }
            }
        }
    }
}
