using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class DailyVote : PlayerVotingBase
    {
        public DailyVote(GameRoom game, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
        }

        public override bool CanView(Role viewer)
        {
            return true;
        }

        public override bool CanVote(Role voter)
        {
            return voter.IsAlive;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            role.IsAlive = false;
            if (role is BaseRole baseRole && baseRole.IsLoved)
                foreach (var other in game.AliveRoles)
                    if (other is BaseRole otherBase && otherBase.IsLoved)
                        otherBase.IsAlive = false;
        }
    }
}
