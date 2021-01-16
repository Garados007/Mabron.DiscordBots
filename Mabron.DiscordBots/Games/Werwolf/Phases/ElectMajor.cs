using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public class ElectMajor : Phase, IDayPhase<ElectMajor>
    {
        public override string Name => "Bürgermeisterwahl";

        public override bool CanExecute(GameRoom game)
        {
            return !game.AliveRoles.Where(x => x.IsMajor).Any();
        }

        public override void Init(GameRoom game)
        {
            base.Init(game);
            AddVoting(new Votings.ElectMajor(game));
        }

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting is Votings.ElectMajor em)
            {
                var ids = em.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(new Votings.ElectMajor(game, ids));
                RemoveVoting(voting);
            }
        }
    }
}
