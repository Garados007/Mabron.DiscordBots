using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class MajorPick : PlayerVotingBase
    {
        public MajorPick(GameRoom game, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
        }

        public override bool CanView(Role viewer)
        {
            return true;
        }

        public override bool CanVote(Role voter)
        {
            return voter.IsMajor && voter.IsAlive;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            role.Kill(game);
        }
    }
}
