using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Votings
{
    public class ElectMajor : PlayerVotingBase
    {
        public ElectMajor(GameRoom game, IEnumerable<ulong>? participants = null)
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
            role.IsMajor = true;
        }
    }
}
