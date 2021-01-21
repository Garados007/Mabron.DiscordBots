using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class AmorPick : PlayerVotingBase
    {
        public AmorPick(GameRoom game, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
        }

        public override bool CanView(Role viewer)
        {
            return viewer is Roles.Amor;
        }

        public override bool CanVote(Role voter)
        {
            return voter is Roles.Amor && voter.IsAlive;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            if (!(role is BaseRole baseRole))
                return;
            baseRole.IsLoved = true;
            if (game.Phase?.Current is Phases.AmorPick pick)
            {
                pick.VotingFinished(this);
            }
        }
    }
}
