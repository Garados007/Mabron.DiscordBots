using System;
using Discord;
using LiteDB;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class GameUser
    {
        public ObjectId Id { get; set; } = new ObjectId();

        public UserId UserId { get; set; }
                
        [BsonIgnore, Obsolete]
        public ulong DiscordId
        {
            get => UserId.Id;
            set => UserId = new UserId(UserId.Source, value);
        }

        public string Username { get; set; } = "";

        public string Image { get; set; } = "";

        public string ThemeColor { get; set; } = "#ffffff";

        public string BackgroundImage { get; set; } = "";

        public string Language { get; set; } = "de";

        // Stats

        public uint StatsWinGames { get; set; } = 0;

        public uint StatsKilled { get; set; } = 0;

        public uint StatsLooseGames { get; set; } = 0;

        public uint StatsLeader { get; set; } = 0;

        public uint Level { get; set; } = 0;

        public ulong CurrentXP { get; set; } = 0;

        public ulong LevelMaxXP 
            => (ulong)(40 * (Math.Pow(Level, 1.2) + Math.Pow(1.1, Math.Pow(Level, 0.5))));

        public static GameUser Create(IUser user)
        {
            var userId = new UserId(UserIdSource.Discord, user.Id);
            var gameUser = Theme.User!.Query()
                .Where(x => x.UserId == userId)
                .FirstOrDefault();
            if (gameUser != null)
            {
                gameUser.Username = user.Username;
                gameUser.Image = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl() ?? "";
                Theme.User.Update(gameUser);
                return gameUser;
            }
            gameUser = new GameUser
            {
                UserId = new UserId
                {
                    Source = UserIdSource.Discord,
                    Id = user.Id,
                },
                Username = user.Username,
                Image = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl() ?? ""
            };
            Theme.User.Insert(gameUser);
            return gameUser;
        }

        public static GameUser? Get(UserIdSource userIdSource, ulong id)
        {
            var userId = new UserId(userIdSource, id);
            return Theme.User!.Query()
                .Where(x => x.UserId == userId)
                .FirstOrDefault();
        }
    }

    public struct UserId : IEquatable<UserId>
    {
        public UserIdSource Source { get; set; }

        public ulong Id { get; set; }

        //public UserId() { }

        public UserId(UserIdSource source, ulong id)
        {
            Source = source;
            Id = id;
        }

        public override bool Equals(object? obj)
        {
            return obj is UserId id && Equals(id);
        }

        public bool Equals(UserId other)
        {
            return Source == other.Source &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Source, Id);
        }

        public static bool operator ==(UserId left, UserId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UserId left, UserId right)
        {
            return !(left == right);
        }
    }

    public enum UserIdSource : byte
    {
        Discord = 0,
        Temporary = 255,
    }
}
