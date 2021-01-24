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

        protected virtual bool AllowDoNothingOption { get; } = false;

        protected int? NoOptionId { get; }

        protected virtual string DoNothingOptionText { get; } = "nichts tun";

        public PlayerVotingBase(GameRoom game, IEnumerable<ulong>? participants = null)
        {
            int index = 0;

            if (AllowDoNothingOption)
            {
                NoOptionId = index++;
                options.TryAdd(NoOptionId.Value, (0, new VoteOption("nichts tun")));
            }
            else NoOptionId = null;

            participants ??= game.Participants
                .Where(x => x.Value != null && DefaultParticipantSelector(x.Value))
                .Select(x => x.Key);

            foreach (var id in participants)
            {
                var name = GetUserString(id, game.UserCache.TryGetValue(id, out GameUser? user) ? user : null);
                options.TryAdd(index++, (id, new VoteOption(name)));
            }
        }

        protected virtual string GetUserString(ulong id, GameUser? user)
            => user?.Username ?? $"User {id}";

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
            if (id != NoOptionId && options.TryGetValue(id, out (ulong user, VoteOption opt) result))
            {
                if (game.Participants.TryGetValue(result.user, out Role? role) && role != null)
                    Execute(game, result.user, role);
            }
        }

        public abstract void Execute(GameRoom game, ulong id, Role role);

        public virtual void RemoveOption(ulong user)
        {
            var key = options
                .Where(x => x.Value.id == user)
                .Select(x => (int?)x.Key)
                .FirstOrDefault();
            if (key != null)
                options.Remove(key.Value, out _);
        }
    }
}
