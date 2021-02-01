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

        ulong leader;
        public ulong Leader
        {
            get => leader;
            set
            {
                Leader = value;
                SendEvent(new Events.OnLeaderChanged(value));
            }
        }

        public bool IsRunning { get; set; } = false;

        public PhaseFlow? Phase { get; private set; }

        public ConcurrentDictionary<ulong, Role?> Participants { get; }

        public ConcurrentDictionary<ulong, GameUser> UserCache { get; }

        public ConcurrentDictionary<Role, int> RoleConfiguration { get; }

        bool leaderIsPlayer = false;
        public bool LeaderIsPlayer
        {
            get => leaderIsPlayer;
            set
            {
                if (leaderIsPlayer == value)
                    return;
                if (value)
                    Participants.TryAdd(Leader, null);
                else Participants.TryRemove(Leader, out _);
                leaderIsPlayer = value;
            }
        }

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
            SendEvent(new Events.AddParticipant(user));
            return true;
        }

        public void RemoveParticipant(GameUser user)
        {
            if (Participants!.Remove(user.DiscordId, out _))
                UserCache.Remove(user.DiscordId, out _);
            SendEvent(new Events.RemoveParticipant(user.DiscordId));
        }

        public IEnumerable<Role> AliveRoles
            => Participants.Values.Where(x => x != null).Cast<Role>().Where(x => x.IsAlive);

        public Role? TryGetRole(ulong id)
        {
            if (Participants.TryGetValue(id, out Role? role))
                return role;
            else return null;
        }

        public bool FullConfiguration => RoleConfiguration.Values.Sum() == Participants.Count;

        public void NextPhase()
        {
            if (!Phase?.Next(this) ?? true)
                Phase = null;
            SendEvent(new Events.NextPhase(Phase?.Current));
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

        public void StopGame(ReadOnlyMemory<Role>? winner)
        {
            ExecutionRound++;

            if (winner != null)
            {
                var winnerSpan = winner.Value.Span;
                var winIds = new List<ulong>(winner.Value.Length);
                // -0.15 is for the leader
                var xpMultiplier = Participants.Values.Where(x => x != null).Count() * 0.15;
                if (leaderIsPlayer) // we have one more player
                    xpMultiplier -= 0.15;
                foreach (var (id, user) in UserCache)
                {
                    if (id == Leader && !LeaderIsPlayer)
                    {
                        user.StatsLeader++;
                        user.CurrentXP += (ulong)Math.Round(xpMultiplier * 100);
                    }
                    if (Participants.TryGetValue(id, out Role? role) && role != null)
                    {
                        if (role.IsAlive)
                        {
                            user.CurrentXP += (ulong)Math.Round(xpMultiplier * 160);
                        }
                        else
                        {
                            user.StatsKilled++;
                        }

                        bool won = false;
                        foreach (var other in winnerSpan)
                            if (other == role)
                            {
                                won = true;
                                break;
                            }
                        if (won)
                        {
                            user.StatsWinGames++;
                            user.CurrentXP += (ulong)Math.Round(xpMultiplier * 120);
                            winIds.Add(id);
                        }
                        else
                        {
                            user.StatsLooseGames++;
                        }
                    }
                    ulong maxXP;
                    while (user.CurrentXP >= (maxXP = user.LevelMaxXP))
                    {
                        user.CurrentXP -= maxXP;
                        user.Level++;
                    }
                    Theme.User!.Update(user);
                }
                Winner = (ExecutionRound, winIds.ToArray());
            }
            IsRunning = false;
            Phase = null;
        }
    
        public event EventHandler<GameEvent>? OnEvent;

        public void SendEvent<T>(T @event)
            where T : GameEvent
        {
            OnEvent?.Invoke(this, @event);
        }
    }
}