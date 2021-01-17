using Mabron.DiscordBots.Games.Werwolf.Phases;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class OraclePick : SingleVotingPhase<Votings.OraclePick>, INightPhase<OraclePick>
    {
        public override string Name => "Alte Seherin";

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Where(x => x is Roles.Oracle).Any();
        }

        protected override Votings.OraclePick Create(GameRoom game, IEnumerable<ulong>? ids = null)
            => new Votings.OraclePick(game, ids);
    }
}
