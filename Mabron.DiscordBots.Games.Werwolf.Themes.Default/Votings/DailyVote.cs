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
            return voter.IsAlive && (!(voter is Roles.Idiot idiot) || !idiot.IsRevealed);
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            if (role is Roles.Idiot idiot)
            {
                idiot.IsRevealed = true;
                idiot.WasMajor = idiot.IsMajor;
                idiot.IsMajor = false;
                return;
            }
            role.Kill(game);
        }
    }
}
