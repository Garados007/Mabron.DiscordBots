using LiteDB;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class VoteOption
    {
        public string LangId { get; }

        public Dictionary<string, string> Vars { get; }

        public ConcurrentBag<ObjectId> Users { get; }
            = new ConcurrentBag<ObjectId>();

        public VoteOption(string langId, params (string key, string value)[] vars)
        {
            LangId = langId;
            Vars = new Dictionary<string, string>();
            foreach (var (key, value) in vars)
                Vars.Add(key, value);
        }

        public VoteOption(string langId, Dictionary<string, string> vars)
        {
            LangId = langId;
            Vars = vars;
        }
    }
}
