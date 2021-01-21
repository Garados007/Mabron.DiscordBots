using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public abstract class SeperateVotingPhaseBase<TVoting, TRole> : Phase
        where TVoting : Voting
        where TRole : Role
    {
        protected abstract TVoting Create(TRole role, GameRoom game);

        protected abstract TRole GetRole(TVoting voting);

        protected abstract bool FilterVoter(TRole role);

        public override bool CanExecute(GameRoom game)
        {
            return game.Participants.Values
                .Where(x => x is TRole role && FilterVoter(role))
                .Any();
        }

        public override void Init(GameRoom game)
        {
            base.Init(game);
            foreach (var role in game.Participants.Values)
                if (role is TRole trole && FilterVoter(trole))
                    AddVoting(Create(trole, game));
        }
    }
}
