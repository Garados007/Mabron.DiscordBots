using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using LiteDB;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class GameController : IDisposable
    {
        public static GameController Current { get; }
            = new GameController();
        
        private GameController() {}

        readonly ConcurrentDictionary<int, GameRoom> rooms = new ConcurrentDictionary<int, GameRoom>();

        readonly HashSet<GameWebSocketConnection> wsConnections
            = new HashSet<GameWebSocketConnection>();
        readonly ReaderWriterLockSlim lockWsConnections
            = new ReaderWriterLockSlim();

        public int CreateGame(GameUser leader)
        {
            var r = new Random();
            int id;
            while (rooms.ContainsKey(id = r.Next())) ;
#if ROOM_ID_1
            // this is a magic value that results in a "Test_" url
            id = unchecked((int)0xfb_2d_eb_4d);
#endif
            var room = new GameRoom(id, leader);
            room.Theme = new Themes.Default.DefaultTheme(room);
            rooms.TryAdd(id, room);
            room.OnEvent += OnGameEvent;
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
            ReadOnlySpan<byte> b1 = BitConverter.GetBytes(game.Id); // 4 B
            ReadOnlySpan<byte> b2 = user.Id.ToByteArray(); // 12 B
            Span<byte> rb = stackalloc byte[16];
            b1.CopyTo(rb[0 .. 4]);
            b2.CopyTo(rb[4 .. 16]);
            return Convert.ToBase64String(rb).Replace('/', '-').Replace('+', '_').TrimEnd('=');
        }

        public (GameRoom game, GameUser user)? GetFromToken(string token)
        {
            token = token.Replace('-', '/').Replace('_', '+') + "==";
            Span<byte> bytes = stackalloc byte[16];
            if (!Convert.TryFromBase64String(token, bytes, out int bytesWritten) || bytesWritten != 16)
                return null;

            int gameId = BitConverter.ToInt32(bytes[0 .. 4]);
            ObjectId userId = new ObjectId(bytes[4..16].ToArray());
            var game = GetGame(gameId);
            if (game == null)
                return null;
            if (!game.UserCache.TryGetValue(userId, out GameUser? user))
                return null;
            return (game, user);
        }

        private void OnGameEvent(object? sender, GameEvent @event)
        {
            lockWsConnections.EnterReadLock();
            foreach (var connection in wsConnections)
                if (connection.Game == sender && @event.CanSendTo(connection.Game, connection.User))
                {
                    _ = Task.Run(async () => await connection.SendEvent(@event));
                }
            lockWsConnections.ExitReadLock();
        }

        public void AddWsConnection(GameWebSocketConnection connection)
        {
            lockWsConnections.EnterWriteLock();
            wsConnections.Add(connection);
            lockWsConnections.ExitWriteLock();
        }

        public void RemoveWsConnection(GameWebSocketConnection connection)
        {
            lockWsConnections.EnterWriteLock();
            wsConnections.Remove(connection);
            lockWsConnections.ExitWriteLock();
        }

        public void Dispose()
        {
            lockWsConnections.Dispose();
        }
    }
}