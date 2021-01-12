using Discord.Rest;
using Discord.WebSocket;
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

        public Phase? Phase { get; private set; }

        public Dictionary<ulong, Role?> Participants { get; }

        public Dictionary<ulong, SocketUser> UserCache { get; }

        public Dictionary<Role, int> RoleConfiguration { get; }

        public bool DeadCanSeeAllRoles { get; set; } = false;

        public GameRoom(int id, SocketUser leader)
        {
            Id = id;
            Leader = leader.Id;
            Participants = new Dictionary<ulong, Role?>();
            UserCache = new Dictionary<ulong, SocketUser>
            {
                [leader.Id] = leader
            };
            RoleConfiguration = new Dictionary<Role, int>();
        }

        public bool AddParticipant(SocketUser user)
        {
            if (Leader == user.Id || Participants.ContainsKey(user.Id))
                return false;

            Participants.Add(user.Id, null);
            UserCache[user.Id] = user;
            return true;
        }

        public void RemoveParticipant(SocketUser user)
        {
            if (Participants.Remove(user.Id))
                UserCache.Remove(user.Id);
        }

        public IEnumerable<Role> AliveRoles
            => Participants.Values.Where(x => x != null).Cast<Role>().Where(x => x.IsAlive);

        public static IEnumerable<Role> GetRoleTemplates()
        {
            yield return new Roles.Villager();
            yield return new Roles.Werwolf();
        }

        public bool FullConfiguration => RoleConfiguration.Values.Sum() == Participants.Count;

        public void NextPhase()
        {
            Phase = Phase.GetNextPhase(this);
            Phase?.Init(this);
        }

        public void StopGame()
        {
            IsRunning = false;
            Phase = null;
        }
    }
}