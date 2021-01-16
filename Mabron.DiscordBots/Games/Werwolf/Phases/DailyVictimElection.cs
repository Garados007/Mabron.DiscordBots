using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public class DailyVictimElection : Phase, IDayPhase<DailyVictimElection>
    {
        public override string Name => "Tag: Dorfwahl";

        public override bool CanExecute(GameRoom game)
        {
            return true;
        }

        public override void Init(GameRoom game)
        {
            base.Init(game);
            AddVoting(new Votings.DailyVote(game));
        }

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting is Votings.DailyVote dv)
            {
                var ids = dv.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(new Votings.MajorPick(game, ids));
                RemoveVoting(voting);
            }
            if (voting is Votings.MajorPick mp)
            {
                var ids = mp.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(new Votings.MajorPick(game, ids));
                RemoveVoting(voting);
            }
        }
    }
}
