using LiteDB;
using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class ElectMajorPhase : Phase, IDayPhase<ElectMajorPhase>
    {
        public class ElectMajor : PlayerVotingBase
        {
            public ElectMajor(GameRoom game, IEnumerable<ObjectId>? participants = null)
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

            public override void Execute(GameRoom game, ObjectId id, Role role)
            {
                role.IsMajor = true;
                game.SendEvent(new Events.PlayerNotification("new-voted-major", new[] { id }));
            }
        }

        public override bool CanExecute(GameRoom game)
        {
            var isMajorRemoved = game.Participants.Values
                .Where(x => x is Roles.Idiot)
                .Cast<Roles.Idiot>()
                .Where(x => x.WasMajor)
                .Any();
            return !isMajorRemoved && !game.AliveRoles.Where(x => x.IsMajor).Any();
        }

        public override void Init(GameRoom game)
        {
            base.Init(game);
            AddVoting(new ElectMajor(game));
        }

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting is ElectMajor em)
            {
                var ids = em.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(new ElectMajor(game, ids));
                RemoveVoting(voting);
            }
        }
    }
}
