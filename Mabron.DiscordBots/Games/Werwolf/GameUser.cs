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

        public static GameUser Create(IUser user)
        {
            var gameUser = GameServer.User!.Query()
                .Where(x => x.DiscordId == user.Id)
                .FirstOrDefault();
            if (gameUser != null)
            {
                gameUser.Username = user.Username;
                gameUser.Image = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl() ?? "";
                GameServer.User.Update(gameUser);
                return gameUser;
            }
            gameUser = new GameUser
            {
                DiscordId = user.Id,
                Username = user.Username,
                Image = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl() ?? ""
            };
            GameServer.User.Insert(gameUser);
            return gameUser;
        }

        public static GameUser? Get(ulong id)
        {
            return GameServer.User!.Query()
                .Where(x => x.DiscordId == id)
                .FirstOrDefault();
        }
    }
}
