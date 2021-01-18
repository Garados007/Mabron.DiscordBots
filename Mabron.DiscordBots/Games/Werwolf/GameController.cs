using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Discord.WebSocket;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class GameController
    {
        public static GameController Current { get; }
            = new GameController();
        
        private GameController() {}

        readonly ConcurrentDictionary<int, GameRoom> rooms = new ConcurrentDictionary<int, GameRoom>();

        public int CreateGame(GameUser leader)
        {
            var r = new Random();
            int id;
            //while (rooms.ContainsKey(id = r.Next())) ;
            id = 1;
            rooms.TryAdd(id, new GameRoom(id, leader) 
            { 
                Theme = new Themes.Default.DefaultTheme(),
            });
            return id;
        }

        public GameRoom? GetGame(int id)
        {
            return rooms.TryGetValue(id, out GameRoom? room)
                ? room
                : null;
        }

        public string GetUserToken(GameRoom game, GameUser user)
        {
            ReadOnlySpan<byte> b1 = BitConverter.GetBytes(game.Id);
            ReadOnlySpan<byte> b2 = BitConverter.GetBytes(user.DiscordId);
            Span<byte> rb = stackalloc byte[12];
            b1.CopyTo(rb[0 .. 4]);
            b2.CopyTo(rb[4 .. 12]);
            return Convert.ToBase64String(rb).Replace('/', '-').Replace('+', '_');
        }

        public (GameRoom game, GameUser user)? GetFromToken(string token)
        {
            token = token.Replace('-', '/').Replace('_', '+');
            Span<byte> bytes = stackalloc byte[12];
            if (!Convert.TryFromBase64String(token, bytes, out int bytesWritten) || bytesWritten != 12)
                return null;

            int gameId = BitConverter.ToInt32(bytes[0 .. 4]);
            ulong userId = BitConverter.ToUInt64(bytes[4 .. 12]);
            var game = GetGame(gameId);
            if (game == null)
                return null;
            if (!game.UserCache.TryGetValue(userId, out GameUser? user))
                return null;
            return (game, user);
        }
    }
}