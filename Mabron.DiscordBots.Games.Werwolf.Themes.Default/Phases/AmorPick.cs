using Mabron.DiscordBots.Games.Werwolf.Phases;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class AmorPick : Phase, INightPhase<AmorPick>
    {
        public override string Name => "Amor sucht das Liebespaar";

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Any(x => x is Roles.Amor) &&
                !game.Participants.Values.Where(x => x is BaseRole role && role.IsLoved).Any();
        }

        Votings.AmorPick? pick1, pick2;

        public override void Init(GameRoom game)
        {
            base.Init(game);
            AddVoting(pick1 = new Votings.AmorPick(game));
            AddVoting(pick2 = new Votings.AmorPick(game));
        }

        public void VotingFinished(Votings.AmorPick voting)
        {
            var result = voting.GetResultUserIds().ToArray();
            if (result.Length == 1)
            {
                if (pick1 == voting)
                    pick2?.RemoveOption(result[0]);
                if (pick2 == voting)
                    pick1?.RemoveOption(result[0]);
            }
        }

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting == pick1)
            {
                var ids = pick1.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(pick1 = new Votings.AmorPick(game, ids));
                RemoveVoting(voting);
            }
            if (voting == pick2)
            {
                var ids = pick2.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(pick2 = new Votings.AmorPick(game, ids));
                RemoveVoting(voting);
            }
        }
    }
}
