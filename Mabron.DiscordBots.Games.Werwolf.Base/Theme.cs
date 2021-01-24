using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

        public abstract IEnumerable<WinConditionCheck> GetWinConditions();

        public virtual bool CheckRoleUsage(Role role, ref int count, int oldCount, 
            [NotNullWhen(false)] out string? error
        )
        {
            if (count < 0)
            {
                error = "invalid number of roles (require >= 0)";
                count = oldCount;
                return false;
            }
            if (count > 500)
            {
                error = "invalid number of roles (require <= 500)";
                count = oldCount;
                return false;
            }
            error = null;
            return true;
        }
    }
}
