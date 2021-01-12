using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public class WerwolfPhase : NightPhaseBase
    {
        public override string Name => "Nacht: Werwölfe";

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Where(x => x is Roles.WerwolfBase).Any();
        }

        public override void Init(GameRoom game)
        {
            base.Init(game);
            AddVoting(new Votings.WerwolfVote(game));
        }

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting is Votings.WerwolfVote wv)
            {
                var ids = wv.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(new Votings.WerwolfVote(game, ids));
            }
        }
    }
}
