using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public abstract class SingleVotingPhase<T> : Phase
        where T : PlayerVotingBase
    {
        protected abstract T Create(GameRoom game, IEnumerable<ulong>? ids = null);

        public override void Init(GameRoom game)
        {
            base.Init(game);
            AddVoting(Create(game));
        }

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting is T tvoting)
            {
                var ids = tvoting.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(Create(game, ids));
                RemoveVoting(tvoting);
            }
        }
    }
}
