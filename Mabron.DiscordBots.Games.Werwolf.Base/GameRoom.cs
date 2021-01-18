using Discord.Rest;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class GameRoom
    {
        public int Id { get; }

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

        public Theme? Theme { get; set; }

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
            if (Phase?.Next(this) ?? false)
                Phase.Current.Init(this);
            else Phase = null;
        }

        public void StartGame()
        {
            Phase = Theme?.GetPhases();
            if (Phase != null && (!Phase.Current.IsGamePhase || !Phase.Current.CanExecute(this)))
                NextPhase();
        }

        public void StopGame()
        {
            IsRunning = false;
            Phase = null;
        }
    }
}