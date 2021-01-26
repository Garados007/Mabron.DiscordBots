using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class HunterPhase : SeperateVotingPhase<HunterPhase.HunterKill, Hunter>
    {
        public class HunterKill : PlayerVotingBase
        {
            public Roles.Hunter Hunter { get; }

            public HunterKill(GameRoom game, Roles.Hunter hunter, IEnumerable<ulong>? participants = null) 
                : base(game, participants)
            {
                Hunter = hunter;
            }

            public override bool CanView(Role viewer)
            {
                return viewer == Hunter;
            }

            public override bool CanVote(Role voter)
            {
                return voter == Hunter;
            }

            public override void Execute(GameRoom game, ulong id, Role role)
            {
                role.Kill(game);
                Hunter.HasKilled = true;
            }
        }

        public override bool CanExecute(GameRoom game)
        {
            return base.CanExecute(game) &&
                !game.Participants.Values.Where(x => x is OldMan oldMan && oldMan.WasKilledByVillager).Any();
        }

        protected override HunterKill Create(Hunter role, GameRoom game, IEnumerable<ulong>? ids = null)
            => new HunterKill(game, role, ids);

        protected override Hunter GetRole(HunterKill voting)
            => voting.Hunter;

        protected override bool FilterVoter(Hunter role)
            => !role.IsAlive && !role.HasKilled;
    }
}
