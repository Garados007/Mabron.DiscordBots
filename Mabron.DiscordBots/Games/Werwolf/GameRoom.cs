using System.Collections.Generic;
using Discord.WebSocket;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class GameRoom
    {
        public int Id { get; }

        public ulong Leader { get; set; }

        public Dictionary<ulong, Role?> Participants { get; }

        public Dictionary<ulong, SocketUser> UserCache { get; }

        public GameRoom(int id, SocketUser leader)
        {
            Id = id;
            Leader = leader.Id;
            Participants = new Dictionary<ulong, Role?>();
            UserCache = new Dictionary<ulong, SocketUser>();
            UserCache[leader.Id] = leader;
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
    }
}