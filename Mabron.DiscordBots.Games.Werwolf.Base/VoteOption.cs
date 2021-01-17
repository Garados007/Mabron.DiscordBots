using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class VoteOption
    {
        public string Name { get; }

        public ConcurrentBag<ulong> Users { get; }
            = new ConcurrentBag<ulong>();

        public VoteOption(string name)
            => Name = name;
    }
}
