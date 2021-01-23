using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class HunterKill : PlayerVotingBase
    {
        public Roles.Hunter Hunter { get; }

        public HunterKill(GameRoom game, Roles.Hunter hunter, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
            Hunter = hunter;
        }

        public override bool CanView(Role viewer)
        {
            return viewer == Hunter;
        }

        public override bool CanVote(Role voter)
        {
            return voter == Hunter;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            role.Kill(game);
            Hunter.HasKilled = true;
        }
    }
}
