using System;
using Discord;
using LiteDB;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class GameUser
    {
        public ObjectId Id { get; set; } = new ObjectId();

        public ulong DiscordId { get; set; }

        public string Username { get; set; } = "";

        public string Image { get; set; } = "";

        public string ThemeColor { get; set; } = "#ffffff";

        public string BackgroundImage { get; set; } = "";

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
            var gameUser = Theme.User!.Query()
                .Where(x => x.DiscordId == user.Id)
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
                DiscordId = user.Id,
                Username = user.Username,
                Image = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl() ?? ""
            };
            Theme.User.Insert(gameUser);
            return gameUser;
        }

        public static GameUser? Get(ulong id)
        {
            return Theme.User!.Query()
                .Where(x => x.DiscordId == id)
                .FirstOrDefault();
        }
    }
}
