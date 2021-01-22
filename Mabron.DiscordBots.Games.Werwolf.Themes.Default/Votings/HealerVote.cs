using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class HealerVote : PlayerVotingBase
    {
        public HealerVote(GameRoom game, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
        }

        protected override bool DefaultParticipantSelector(Role role)
            => role.IsAlive && role is BaseRole baseRole && !baseRole.IsSelectedByHealer;

        public override bool CanView(Role viewer)
            => viewer is Roles.Healer;

        public override bool CanVote(Role voter)
            => voter is Roles.Healer && voter.IsAlive;

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            foreach (var other in game.Participants.Values)
                if (other is BaseRole otherBase)
                    otherBase.IsSelectedByHealer = false;
            if (role is BaseRole baseRole)
                baseRole.IsSelectedByHealer = true;
        }
    }
}
