using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public class WerwolfPhase : SingleVotingPhase<Votings.WerwolfVote>, INightPhase<WerwolfPhase>
    {
        public override string Name => "Nacht: Werwölfe";

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Where(x => x is Roles.WerwolfBase).Any();
        }

        protected override Votings.WerwolfVote Create(GameRoom game, IEnumerable<ulong>? ids = null)
            => new Votings.WerwolfVote(game, ids);
    }
}
