using Mabron.DiscordBots.Games.Werwolf.Phases;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class ThreeBrotherDiscussionPhase : DiscussionPhase
    {
        public override bool CanMessage(GameRoom game, Role role)
            => CanVote(role);

        protected override bool CanView(Role viewer)
        {
            return viewer is Roles.ThreeBrothers;
        }

        protected override bool CanVote(Role voter)
        {
            return voter is Roles.ThreeBrothers && voter.IsAlive;
        }
    }
}
