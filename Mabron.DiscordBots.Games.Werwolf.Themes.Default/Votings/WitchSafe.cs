using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class WitchSafe : Voting
    {
        readonly ConcurrentDictionary<int, (ulong? user, VoteOption option)> options;

        public Roles.Witch Witch { get; }

        public WitchSafe(Roles.Witch witch, IEnumerable<(ulong user, string name)> victims)
        {
            options = new ConcurrentDictionary<int, (ulong? user, VoteOption option)>();
            foreach (var (user, name) in victims)
                options.TryAdd(options.Count, (user, new VoteOption($"{name} retten")));
            options.TryAdd(options.Count, (null, new VoteOption("nichts tun")));
            Witch = witch;
        }

        public override string Name => "Lebenstrank nutzen";

        public override IEnumerable<(int id, VoteOption option)> Options
            => options.Select(x => (x.Key, x.Value.option));

        public override bool CanView(Role viewer)
        {
            return viewer is Roles.Witch;
        }

        public override bool CanVote(Role voter)
        {
            return voter == Witch && !Witch.UsedLivePotion;
        }

        public void RemoveUserOption(ulong user)
        {
            var key = options.Where(x => x.Value.user == user)
                .Select(x => (int?)x.Key)
                .FirstOrDefault();
            if (key != null)
                options.Remove(key.Value, out _);
        }

        public override void Execute(GameRoom game, int id)
        {
            if (options.TryGetValue(id, out (ulong? user, VoteOption option) value)
                && value.user != null
                && game.Participants.TryGetValue(value.user.Value, out Role? role)
                && role is BaseRole baseRole)
            {
                baseRole.IsSelectedByWerewolves = false;
                Witch.UsedLivePotion = true;
                if (game.Phase?.Current is Phases.WitchPhase phase)
                {
                    phase.VotingFinished(this, value.user.Value);
                }
            }
        }
    }
}
