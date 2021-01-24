using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles
{
    public class OldMan : VillagerBase
    {
        public bool WasKilledByWolvesOneTime { get; set; } = false;

        public bool WasKilledByVillager { get; set; } = false;

        public override void Reset()
        {
            base.Reset();
            WasKilledByWolvesOneTime = false; 
            WasKilledByVillager = false;
        }

        public OldMan(Theme theme) : base(theme)
        {
        }

        public override string Name => "Der Alte";

        public override string Description => "Der Alte muss zweimal von den Werwölfen gerissen werden";

        public override Role CreateNew()
            => new OldMan(Theme);

        public override void Kill(GameRoom game)
        {
            base.Kill(game);
            var idiots = game.AliveRoles
                .Where(x => x is Idiot idiot && idiot.IsRevealed);
            foreach (var idiot in idiots)
                idiot.Kill(game);
        }
    }
}
