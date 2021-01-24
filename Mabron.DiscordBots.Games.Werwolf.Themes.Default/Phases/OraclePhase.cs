using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class OraclePhase : SingleVotingPhase<OraclePhase.OraclePick>, INightPhase<OraclePhase>
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

        public override string Name => "Alte Seherin";

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Where(x => x is Roles.Oracle).Any();
        }

        protected override OraclePick Create(GameRoom game, IEnumerable<ulong>? ids = null)
            => new OraclePick(game, ids);
    }
}
