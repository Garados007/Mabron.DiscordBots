using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class WitchSafe : PlayerVotingBase
    {
        public Roles.Witch Witch { get; }

        public WitchSafe(Roles.Witch witch, GameRoom game, IEnumerable<ulong>? participants = null)
            : base(game, participants)
        {
            Witch = witch;
        }

        protected override bool DefaultParticipantSelector(Role role)
        {
            return role is BaseRole baseRole && baseRole.IsSelectedByWerewolves;
        }

        protected override bool AllowDoNothingOption => true;

        protected override string GetUserString(ulong id, GameUser? user)
        {
            return $"{base.GetUserString(id, user)} retten";
        }

        public override string Name => "Lebenstrank nutzen";

        public override bool CanView(Role viewer)
        {
            return viewer == Witch;
        }

        public override bool CanVote(Role voter)
        {
            return voter == Witch;
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            if (role is BaseRole baseRole)
            {
                baseRole.IsSelectedByWerewolves = false;
                Witch.UsedLivePotion = true;
            }
        }
    }
}
