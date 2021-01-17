using Mabron.DiscordBots.Games.Werwolf.Phases;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class HunterKill : SingleVotingPhase<Votings.HunterKill>
    {
        public override string Name => "Rache des Jägers";

        public override bool CanExecute(GameRoom game)
        {
            return game.Participants.Values
                .Where(x => x != null && x is Roles.Hunter hunter && !x.IsAlive && !hunter.HasKilled)
                .Any();
        }

        protected override Votings.HunterKill Create(GameRoom game, IEnumerable<ulong>? ids = null)
            => new Votings.HunterKill(game, ids);
    }
}
