using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class VoteOption
    {
        public string Name { get; }

        public List<ulong> Users { get; }
            = new List<ulong>();

        public VoteOption(string name)
            => Name = name;
    }
}
