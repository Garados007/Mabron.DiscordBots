using System;
using Discord.Rest;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using System.Threading;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class GameRoom
    {
        public int Id { get; }

        public uint ExecutionRound { get; private set; } = 0;

        public RestUserMessage? Message { get; set; }

        ObjectId leader = new ObjectId();
        public ObjectId Leader
        {
            get => leader;
            set
            {
                SendEvent(new Events.OnLeaderChanged(leader = value));
            }
        }

        public PhaseFlow? Phase { get; private set; }

        public ConcurrentDictionary<ObjectId, Role?> Participants { get; }

        public ConcurrentDictionary<ObjectId, GameUser> UserCache { get; }

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

        public bool AllCanSeeRoleOfDead { get; set; } = false;

        public bool AutostartVotings { get; set; } = false;

        public bool AutoFinishVotings { get; set; } = false;

        public bool UseVotingTimeouts { get; set; } = false;

        public bool AutoFinishRounds { get; set; } = false;

        public Theme? Theme { get; set; }

        public (uint round, ReadOnlyMemory<ObjectId> winner)? Winner { get; set; }

        public GameRoom(int id, GameUser leader)
        {
            Id = id;
            Leader = leader.Id;
            Participants = new ConcurrentDictionary<ObjectId, Role?>();
            UserCache = new ConcurrentDictionary<ObjectId, GameUser>()
            {
                [leader.Id] = leader
            };
            RoleConfiguration = new ConcurrentDictionary<Role, int>();
        }

        public bool AddParticipant(GameUser user)
        {
            if (Leader == user.Id || Participants.ContainsKey(user.Id))
                return false;

            Participants.TryAdd(user.Id, null);
            UserCache[user.Id] = user;
            SendEvent(new Events.AddParticipant(user));
            return true;
        }

        public void RemoveParticipant(GameUser user)
        {
            if (Participants!.Remove(user.Id, out _))
                UserCache.Remove(user.Id, out _);
            SendEvent(new Events.RemoveParticipant(user.Id));
        }

        /// <summary>
        /// Any existing roles that are consideres as alive. All close to death roles are excluded.
        /// </summary>
        public IEnumerable<Role> AliveRoles
            => Participants.Values.Where(x => x != null).Cast<Role>().Where(x => x.IsAlive);

        /// <summary>
        /// Any existing roles that are not finally dead. Only roles that have the
        /// <see cref="KillState.Killed"/> are excluded.
        /// </summary>
        public IEnumerable<Role> NotKilledRoles
            => Participants.Values.Where(x => x != null).Cast<Role>().Where(x => x.KillState != KillState.Killed);

        public Role? TryGetRole(ObjectId id)
        {
            if (Participants.TryGetValue(id, out Role? role))
                return role;
            else return null;
        }

        public ObjectId? TryGetId(Role role)
        {
            foreach (var (id, prole) in Participants)
                if (prole == role)
                    return id;
            return null;
        }

        public bool FullConfiguration => RoleConfiguration.Values.Sum() == Participants.Count;

        int lockNextPhase = 0;
        public void NextPhase()
        {
            if (Interlocked.Exchange(ref lockNextPhase, 1) != 0)
                return;
            if (!Phase?.Next(this) ?? true)
                Phase = null;
            Interlocked.Exchange(ref lockNextPhase, 0);
        }

        public void StartGame()
        {
            Winner = null;
            ExecutionRound++;
            SendEvent(new Events.GameStart());
            // Setup phases
            Phase = Theme?.GetPhases(RoleConfiguration);
            if (Phase != null && (!Phase.Current.IsGamePhase || !Phase.Current.CanExecute(this)))
                NextPhase();
            // update user cache
            var users = UserCache.Keys.ToArray();
            foreach (var user in users)
                UserCache[user] = Theme.User!.Query()
                    .Where(x => x.Id == user)
                    .First();
            // post init
            Theme?.PostInit(this);
        }

        public void StopGame(ReadOnlyMemory<Role>? winner)
        {
            ExecutionRound++;

            if (winner != null)
            {
                var winnerSpan = winner.Value.Span;
                var winIds = new List<ObjectId>(winner.Value.Length);
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
            Phase = null;
            SendEvent(new Events.GameEnd());
            foreach (var role in Participants.Values)
                if (role != null)
                    SendEvent(new Events.OnRoleInfoChanged(role, ExecutionRound));
        }
    
        public event EventHandler<GameEvent>? OnEvent;

        public void SendEvent<T>(T @event)
            where T : GameEvent
        {
            OnEvent?.Invoke(this, @event);
        }
    }
}