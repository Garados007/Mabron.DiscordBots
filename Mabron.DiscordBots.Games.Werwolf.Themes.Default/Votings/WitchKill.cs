using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class WitchKill : PlayerVotingBase
    {
        public Roles.Witch Witch { get; }

        public WitchKill(Roles.Witch witch, GameRoom game, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
            Witch = witch;
        }

        protected override bool DefaultParticipantSelector(Role role)
        {
            return role.IsAlive && role is BaseRole baseRole &&
                !baseRole.IsSelectedByWerewolves;
        }

        public override string Name => "Todestrank nutzen";

        protected override bool AllowDoNothingOption => true;

        public override bool CanView(Role viewer)
        {
            return viewer is Roles.Witch;
        }

        public override bool CanVote(Role voter)
        {
            return voter == Witch && !Witch.UsedDeathPotion;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            if (role is BaseRole baseRole)
            {
                baseRole.Kill(game);
                Witch.UsedDeathPotion = true;
            }
        }
    }
}
