using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public class InheritMajor : Phase
    {
        public override string Name => "Vererbung des Bürgermeisters";

        public override bool CanExecute(GameRoom game)
        {
            return game.Participants.Values
                .Where(x => x != null && x.IsMajor && !x.IsAlive)
                .Any();
        }

        public override void Init(GameRoom game)
        {
            base.Init(game);
            AddVoting(new Votings.InheritMajor(game));
        }

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting is Votings.InheritMajor im)
            {
                var ids = im.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(new Votings.InheritMajor(game, ids));
            }
        }
    }
}
