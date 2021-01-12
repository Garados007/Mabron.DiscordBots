using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Votings
{
    public class WerwolfVote : PlayerVotingBase
    {
        public WerwolfVote(GameRoom game, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
        }

        protected override bool DefaultParticipantSelector(Role role)
        {
            return !(role is Roles.WerwolfBase) && role.IsAlive;
        }

        public override string Name => "Nächtliche Opferauswahl";

        public override bool CanView(Role viewer)
        {
            return viewer is Roles.WerwolfBase;
        }

        public override bool CanVote(Role voter)
        {
            return voter is Roles.WerwolfBase && voter.IsAlive;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            role.IsSelectedByWerewolves = true;
        }
    }
}
