using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class HunterKill : SeperateVotingPhase<Votings.HunterKill, Hunter>
    {
        public override string Name => "Rache des Jägers";

        protected override Votings.HunterKill Create(Hunter role, GameRoom game, IEnumerable<ulong>? ids = null)
            => new Votings.HunterKill(game, role, ids);

        protected override Hunter GetRole(Votings.HunterKill voting)
            => voting.Hunter;

        protected override bool FilterVoter(Hunter role)
            => !role.IsAlive && !role.HasKilled;
    }
}
