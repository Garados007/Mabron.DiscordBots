using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class OraclePick : PlayerVotingBase
    {
        public OraclePick(GameRoom game, IEnumerable<ulong>? participants = null)
            : base(game, participants)
        {
        }

        protected override bool DefaultParticipantSelector(Role role)
        {
            return role is BaseRole baseRole &&
                role.IsAlive && !(role is Roles.Oracle) && !baseRole.IsViewedByOracle;
        }

        public override bool CanView(Role viewer)
        {
            return viewer is Roles.Oracle;
        }

        public override bool CanVote(Role voter)
        {
            return voter is Roles.Oracle && voter.IsAlive;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            if (role is BaseRole baseRole)
                baseRole.IsViewedByOracle = true;
        }
    }
}
