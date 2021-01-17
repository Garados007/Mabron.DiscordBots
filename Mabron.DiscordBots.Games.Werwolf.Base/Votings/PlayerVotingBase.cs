using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Votings
{
    public abstract class PlayerVotingBase : Voting
    {
        protected readonly ConcurrentDictionary<int, (ulong id, VoteOption opt)> options
            = new ConcurrentDictionary<int, (ulong id, VoteOption opt)>();

        public override IEnumerable<(int id, VoteOption option)> Options
            => options.Select(x => (x.Key, x.Value.opt));

        public override string Name => "Wähle einen Spieler";

        public PlayerVotingBase(GameRoom game, IEnumerable<ulong>? participants = null)
        {
            int index = 0;
            participants ??= game.Participants
                .Where(x => x.Value != null && DefaultParticipantSelector(x.Value))
                .Select(x => x.Key);
            foreach (var id in participants)
            {
                var name = game.UserCache.TryGetValue(id, out GameUser? user) ?
                    user.Username :
                    $"User {id}";
                options.TryAdd(index++, (id, new VoteOption(name)));
            }
        }

        protected virtual bool DefaultParticipantSelector(Role role)
        {
            return role.IsAlive;
        }

        public IEnumerable<ulong> GetResultUserIds()
        {
            return GetResults()
                .Select(x => options.TryGetValue(x, out (ulong, VoteOption) r) ? (ulong?)r.Item1 : null)
                .Where(x => x != null)
                .Select(x => x!.Value);
        }

        public sealed override void Execute(GameRoom game, int id)
        {
            if (options.TryGetValue(id, out (ulong user, VoteOption opt) result))
            {
                if (game.Participants.TryGetValue(result.user, out Role? role) && role != null)
                    Execute(game, result.user, role);
            }
        }

        public abstract void Execute(GameRoom game, ulong id, Role role);
    }
}
