using Mabron.DiscordBots.Games.Werwolf.Phases;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class HealerVote : SingleVotingPhase<Votings.HealerVote>, INightPhase<HealerVote>
    {
        public override string Name => "Heiler wendet Praktiken an";

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Where(x => x is Roles.Healer).Any();
        }

        protected override Votings.HealerVote Create(GameRoom game, IEnumerable<ulong>? ids = null)
            => new Votings.HealerVote(game, ids);
    }
}
