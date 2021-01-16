using System;
using System.Collections.Generic;
using System.Text;

namespace Mabron.DiscordBots.Games.Werwolf.Votings
{
    public class HunterKill : PlayerVotingBase
    {
        public HunterKill(GameRoom game, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
        }

        public override bool CanView(Role viewer)
        {
            return viewer is Roles.Hunter;
        }

        public override bool CanVote(Role voter)
        {
            return voter is Roles.Hunter hunter && !hunter.IsAlive && !hunter.HasKilled;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            role.IsAlive = false;
            if (role.IsLoved)
                foreach (var other in game.AliveRoles)
                    if (other.IsLoved)
                        other.IsAlive = false;
        }
    }
}
