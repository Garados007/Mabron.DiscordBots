using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class ScapeGoatPhase : SeperateVotingPhase<ScapeGoatPhase.ScapeGoatSelect, ScapeGoat>
    {
        public class ScapeGoatSelect : PlayerVotingBase
        {
            public ScapeGoat ScapeGoat { get; }

            public ScapeGoatSelect(ScapeGoat scapeGoat, GameRoom game, IEnumerable<ulong>? participants = null) 
                : base(game, participants)
            {
                ScapeGoat = scapeGoat;
            }

            protected override bool AllowDoNothingOption => true;

            protected override string DoNothingOptionText => "Abstimmung beenden";

            public override string Name => "Verteile Stimmrechte";

            private bool CanFinishVoting = false;

            public override bool CanView(Role viewer)
            {
                return true;
            }

            public override bool CanVote(Role voter)
            {
                return voter == ScapeGoat;
            }

            public override void Execute(GameRoom game, ulong id, Role role)
            {
                if (role is BaseRole baseRole)
                {
                    baseRole.HasVotePermitFromScapeGoat = true;
                }
            }

            protected override int GetMissingVotes(GameRoom game)
            {
                return CanFinishVoting ? 0 : 1;
            }

            public override string? Vote(GameRoom game, ulong voter, int id)
            {
                if (CanFinishVoting)
                    return "You already selected to be finished";

                var option = Options
                    .Where(x => x.id == id)
                    .Select(x => x.option)
                    .FirstOrDefault();

                if (option == null)
                    return "option not found";

                string? error;
                if ((error = Vote(game, voter, option)) != null)
                    return error;

                if (id == 0)
                    CanFinishVoting = true;

                CheckVotingFinished(game);

                return null;
            }
        }

        public override bool CanExecute(GameRoom game)
        {
            return base.CanExecute(game) &&
                !game.Participants.Values.Where(x => x is OldMan oldMan && oldMan.WasKilledByVillager).Any();
        }

        public override string Name => "Auswahl der Stimmberechtigten";

        protected override ScapeGoatSelect Create(ScapeGoat role, GameRoom game, IEnumerable<ulong>? ids = null)
            => new ScapeGoatSelect(role, game, ids);

        protected override bool FilterVoter(ScapeGoat role)
            => !role.IsAlive && role.WasKilledByVillage && !role.HasRevenge;

        protected override ScapeGoat GetRole(ScapeGoatSelect voting)
            => voting.ScapeGoat;

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting is ScapeGoatSelect select)
            {
                foreach (var id in select.GetResultUserIds())
                    if (game.Participants.TryGetValue(id, out Role? role) && role != null)
                        select.Execute(game, id, role);
            }
        }
    }
}
