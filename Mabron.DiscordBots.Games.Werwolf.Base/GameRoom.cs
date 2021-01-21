using System;
using Discord.Rest;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class GameRoom
    {
        public int Id { get; }

        public uint ExecutionRound { get; private set; } = 0;

        public RestUserMessage? Message { get; set; }

        public ulong Leader { get; set; }

        public bool IsRunning { get; set; } = false;

        public PhaseFlow? Phase { get; private set; }

        public ConcurrentDictionary<ulong, Role?> Participants { get; }

        public ConcurrentDictionary<ulong, GameUser> UserCache { get; }

        public ConcurrentDictionary<Role, int> RoleConfiguration { get; }

        public bool DeadCanSeeAllRoles { get; set; } = false;

        public bool AutostartVotings { get; set; } = false;

        public bool AutoFinishVotings { get; set; } = false;

        public bool UseVotingTimeouts { get; set; } = false;

        public bool AutoFinishRounds { get; set; } = false;

        public Theme? Theme { get; set; }

        public (uint round, ReadOnlyMemory<ulong> winner)? Winner { get; set; }

        public GameRoom(int id, GameUser leader)
        {
            Id = id;
            Leader = leader.DiscordId;
            Participants = new ConcurrentDictionary<ulong, Role?>();
            UserCache = new ConcurrentDictionary<ulong, GameUser>()
            {
                [leader.DiscordId] = leader
            };
            RoleConfiguration = new ConcurrentDictionary<Role, int>();
        }

        public bool AddParticipant(GameUser user)
        {
            if (Leader == user.DiscordId || Participants.ContainsKey(user.DiscordId))
                return false;

            Participants.TryAdd(user.DiscordId, null);
            UserCache[user.DiscordId] = user;
            return true;
        }

        public void RemoveParticipant(GameUser user)
        {
            if (Participants!.Remove(user.DiscordId, out _))
                UserCache.Remove(user.DiscordId, out _);
        }

        public IEnumerable<Role> AliveRoles
            => Participants.Values.Where(x => x != null).Cast<Role>().Where(x => x.IsAlive);

        public bool FullConfiguration => RoleConfiguration.Values.Sum() == Participants.Count;

        public void NextPhase()
        {
            if (!Phase?.Next(this) ?? true)
                Phase = null;
        }

        public void StartGame()
        {
            Winner = null;
            ExecutionRound++;
            // Setup phases
            Phase = Theme?.GetPhases();
            if (Phase != null && (!Phase.Current.IsGamePhase || !Phase.Current.CanExecute(this)))
                NextPhase();
            // update user cache
            var users = UserCache.Keys.ToArray();
            foreach (var user in users)
                UserCache[user] = Theme.User!.Query()
                    .Where(x => x.DiscordId == user)
                    .First();
        }

        public void StopGame(bool updateStats)
        {
            static bool SameFaction(Role role1, Role role2)
            {
                var same = role1.IsSameFaction(role2);
                if (same == null)
                    same = role2.IsSameFaction(role1);
                return same ?? false;
            }

            ExecutionRound++;

            if (updateStats)
            {
                var winRoles  = AliveRoles.ToArray();
                var winner = new List<ulong>(UserCache.Count);
                foreach (var (id, user) in UserCache)
                {
                    if (id == Leader)
                    {
                        user.StatsLeader++;
                    }
                    if (Participants.TryGetValue(id, out Role? role) && role != null)
                    {
                        if (role.IsAlive)
                        {
                            user.StatsWinGames++;
                            winner.Add(id);
                        }
                        else
                        {
                            user.StatsKilled++;
                            if (winRoles.Any(x => !SameFaction(x, role)))
                                user.StatsLooseGames++;
                            else
                            {
                                user.StatsWinGames++;
                                winner.Add(id);
                            }
                        }
                    }
                    Theme.User!.Update(user);
                }
                Winner = (ExecutionRound, winner.ToArray());
            }
            IsRunning = false;
            Phase = null;
        }
    }
}