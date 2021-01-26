using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class WerwolfPhase : SingleVotingPhase<WerwolfPhase.WerwolfVote>, INightPhase<WerwolfPhase>
    {
        public class WerwolfVote : PlayerVotingBase
        {
            public WerwolfVote(GameRoom game, IEnumerable<ulong>? participants = null) 
                : base(game, participants)
            {
            }

            protected override bool DefaultParticipantSelector(Role role)
            {
                return !(role is WerwolfBase) && role.IsAlive;
            }

            public override bool CanView(Role viewer)
            {
                return viewer is WerwolfBase;
            }

            public override bool CanVote(Role voter)
            {
                return voter is WerwolfBase && voter.IsAlive;
            }

            public override void Execute(GameRoom game, ulong id, Role role)
            {
                if (role is BaseRole baseRole)
                    baseRole.IsSelectedByWerewolves = true;
            }
        }

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Where(x => x is WerwolfBase).Any();
        }

        protected override WerwolfVote Create(GameRoom game, IEnumerable<ulong>? ids = null)
            => new WerwolfVote(game, ids);
    }
}
