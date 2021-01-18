using Mabron.DiscordBots.Games.Werwolf.Phases;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class WitchPhase : Phase, INightPhase<WitchPhase>
    {
        public override string Name => "Braustunde der Hexe";

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Any(x => x is Roles.Witch witch && !(witch.UsedDeathPotion && witch.UsedLivePotion));
        }

        readonly ConcurrentDictionary<int, Votings.WitchSafe> witchSaves 
            = new ConcurrentDictionary<int, Votings.WitchSafe>();
        readonly ConcurrentDictionary<int, Votings.WitchKill> witchKills
            = new ConcurrentDictionary<int, Votings.WitchKill>();

        public override void Init(GameRoom game)
        {
            base.Init(game);
            var victims = game.Participants
                .Where(x => x.Value is BaseRole role && role.IsSelectedByWerewolves)
                .Select(x => (id: x.Key, name: game.UserCache.TryGetValue(x.Key, out GameUser? user) ? user.Username : x.Key.ToString()))
                .ToArray();
            foreach (var role in game.AliveRoles)
                if (role is Roles.Witch witch)
                {
                    if (!witch.UsedLivePotion)
                    {
                        var voting = new Votings.WitchSafe(witch, victims);
                        witchSaves.TryAdd(witchSaves.Count, voting);
                        AddVoting(voting);
                    }
                    if (!witch.UsedDeathPotion)
                    {
                        var voting = new Votings.WitchKill(witch, game);
                        witchKills.TryAdd(witchKills.Count, voting);
                        AddVoting(voting);
                    }
                }
        }

        static void TryRemoveValue<T>(ConcurrentDictionary<int, T> dict, T value)
        {
            var key = dict.Where(x => Equals(x.Value, value))
                .Select(x => (int?)x.Key)
                .FirstOrDefault();
            if (key != null)
                dict.Remove(key.Value, out _);
        }

        public void VotingFinished(Votings.WitchSafe voting, ulong user)
        {
            TryRemoveValue(witchSaves, voting);
            foreach (var (_, other) in witchSaves)
                other.RemoveUserOption(user);
        }

        public void VotingFinished(Votings.WitchKill voting, ulong user)
        {
            TryRemoveValue(witchKills, voting);
            foreach (var (_, other) in witchKills)
                other.RemoveUserOption(user);
        }
    }
}
