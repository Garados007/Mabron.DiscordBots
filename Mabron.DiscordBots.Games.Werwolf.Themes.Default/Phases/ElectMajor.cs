using Mabron.DiscordBots.Games.Werwolf.Phases;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class ElectMajor : Phase, IDayPhase<ElectMajor>
    {
        public override string Name => "Bürgermeisterwahl";

        public override bool CanExecute(GameRoom game)
        {
            var isMajorRemoved = game.Participants.Values
                .Where(x => x is Roles.Idiot)
                .Cast<Roles.Idiot>()
                .Where(x => x.WasMajor)
                .Any();
            return !isMajorRemoved && !game.AliveRoles.Where(x => x.IsMajor).Any();
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
