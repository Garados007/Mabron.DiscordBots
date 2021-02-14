using LiteDB;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public abstract class Theme
    {
        private static LiteDatabase? database;
        public static ILiteCollection<GameUser>? User { get; private set; }

        public string LanguageTheme { get; set; } = "default";

        public static void SetupDB()
        {
            database = new LiteDatabase("game.werwolf.litedb");
            if (database.UserVersion == 0)
            {
                var user = database.GetCollection<GameUser>("user");
                user.DropIndex("DiscordId");
                user.UpdateMany(
                    BsonExpression.Create("{UserId:{Source:\"Discord\",_id:$.DiscordId}}"),
                    BsonExpression.Create("1=1")
                );
                database.UserVersion = 1;
            }
            User = database.GetCollection<GameUser>("user");
            User.EnsureIndex(x => x.UserId, true);
        }

        public abstract Role GetBasicRole();

        public abstract IEnumerable<Role> GetRoleTemplates();

        public abstract PhaseFlow GetPhases(IDictionary<Role, int> roles);

        public abstract IEnumerable<WinConditionCheck> GetWinConditions();

        public GameRoom? Game { get; }

        public Theme(GameRoom? game)
            => Game = game;

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

        public virtual void PostInit(GameRoom game)
        {

        }
    }
}
