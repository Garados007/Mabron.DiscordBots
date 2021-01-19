using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public abstract class Voting
    {
        static ulong nextId = 0;
        public ulong Id { get; }

        public Voting()
        {
            Id = unchecked(nextId++);
        }

        public abstract string Name { get; }

        public bool Started { get; set; }

        private int finished = 0;
        public bool Finished => finished > 0;

        public DateTime? Timeout { get; private set; }

        public abstract IEnumerable<(int id, VoteOption option)> Options { get; }

        public abstract bool CanView(Role viewer);

        public abstract bool CanVote(Role voter);

        public bool SetTimeout(GameRoom game, bool force)
        {
            var count = game.Participants
                .Where(x => x.Value != null && CanVote(x.Value))
                .Where(x => !Options.Any(y => y.option.Users.Contains(x.Key)))
                .Count();

            var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(45 * count);
            if (!force && Timeout != null && timeout - Timeout.Value < TimeSpan.FromSeconds(5))
                return false;

            Timeout = timeout;
            var gameStep = game.ExecutionRound;
            _ = Task.Run(async () =>
            {
                await Task.Delay(timeout - DateTime.UtcNow);
                if (game.ExecutionRound == gameStep && Timeout.Value == timeout)
                {
                    // Timeout exceeded, can now skip
                    FinishVoting(game);
                }
            });
            return true;
        }

        public void FinishVoting(GameRoom game)
        {
            if (Interlocked.Exchange(ref finished, 1) > 0)
                return;
            var vote = GetResult();
            if (vote != null)
            {
                Execute(game, vote.Value);
                game.Phase!.Current.RemoveVoting(this);

                if (new WinCondition().Check(game))
                {
                    game.StopGame(true);
                }
            }
            else
            {
                game.Phase!.Current.ExecuteMultipleWinner(this, game);
            }
        }

        public IEnumerable<Role> GetVoter(GameRoom game)
        {
            foreach (var role in game.Participants.Values)
                if (role != null && CanVote(role))
                    yield return role;
        }

        public virtual int? GetResult()
        {
            var options = GetResults().ToArray();
            if (options.Length == 1)
                return options[0];
            else return null;
        }

        public virtual IEnumerable<int> GetResults()
        {
            var hasEntries = Options.Any();
            if (!hasEntries)
                return Options.Select(x => x.id);
            int max = Options.Max(x => x.option.Users.Count);
            return Options.Where(x => x.option.Users.Count == max)
                .Select(x => x.id);
        }

        public abstract void Execute(GameRoom game, int id);
    }
}
