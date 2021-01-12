using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Votings
{
    public class InheritMajor : PlayerVotingBase
    {
        public InheritMajor(GameRoom game, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
        }

        protected override bool DefaultParticipantSelector(Role role)
        {
            return role.IsAlive && !role.IsMajor;
        }

        public override string Name => "Bürgermeister vererben.";

        public override bool CanView(Role viewer)
        {
            return viewer.IsMajor;
        }

        public override bool CanVote(Role voter)
        {
            return voter.IsMajor && !voter.IsAlive;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            foreach (var entry in game.Participants.Values)
                if (entry != null)
                    entry.IsMajor = false;
            role.IsMajor = true;
        }
    }
}
