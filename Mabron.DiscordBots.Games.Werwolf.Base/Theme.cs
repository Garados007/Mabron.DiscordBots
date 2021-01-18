using LiteDB;
using System;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public abstract class Theme
    {
        private static LiteDatabase? database;
        public static ILiteCollection<GameUser>? User { get; private set; }

        public static void SetupDB()
        {
            database = new LiteDatabase("game.werwolf.litedb");
            User = database.GetCollection<GameUser>("user");
            User.EnsureIndex(x => x.DiscordId, true);
        }

        public abstract Role GetBasicRole();

        public abstract IEnumerable<Role> GetRoleTemplates();

        public abstract PhaseFlow GetPhases();

        public abstract IEnumerable<Func<GameRoom, bool>> GetWinConditions();
    }
}
