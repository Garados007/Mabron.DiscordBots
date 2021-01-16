﻿using System.Collections.Generic;
using System.Linq;

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

        public abstract IEnumerable<(int id, VoteOption option)> Options { get; }

        public abstract bool CanView(Role viewer);

        public abstract bool CanVote(Role voter);

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
