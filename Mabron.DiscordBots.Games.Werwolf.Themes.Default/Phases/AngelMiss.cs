namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class AngelMiss : Werwolf.Phases.ActionPhaseBase
    {
        public override void Execute(GameRoom game)
        {
            foreach (var role in game.Participants.Values)
                if (role is Roles.Angel angel)
                    angel.MissedFirstRound = true;
        }
    }
}
