using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class FlutistPhase : Phase, INightPhase<FlutistPhase>
    {
        public class FlutistPick : PlayerVotingBase
        {
            public GameRoom Game { get; }

            public FlutistPick(GameRoom game, IEnumerable<ulong>? participants = null) 
                : base(game, participants)
            {
                Game = game;
            }

            protected override bool DefaultParticipantSelector(Role role)
            {
                return role is BaseRole baseRole && baseRole.IsAlive &&
                    !baseRole.IsEnchantedByFlutist && !(role is Roles.Flutist);
            }

            public override bool CanView(Role viewer)
            {
                return viewer is Roles.Flutist;
            }

            public override bool CanVote(Role voter)
            {
                return voter is Roles.Flutist && voter.IsAlive;
            }

            public override void Execute(GameRoom game, ulong id, Role role)
            {
                if (!(role is BaseRole baseRole))
                    return;
                baseRole.IsEnchantedByFlutist = true;
                if (game.Phase?.Current is FlutistPhase pick)
                {
                    pick.VotingFinished(this);
                }
            }

            public override void RemoveOption(ulong user)
            {
                base.RemoveOption(user);

                if (options.Count == 0)
                    CheckVotingFinished(Game);
            }
        }

        public override string Name => "Flötenspieler sammelt Jünger";

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Any(x => x is Roles.Flutist);
        }

        readonly List<FlutistPick> picks = new List<FlutistPick>();

        public override void Init(GameRoom game)
        {
            base.Init(game);
            picks.Clear();
            for (int i = 0; i < 2; ++i)
            {
                var pick = new FlutistPick(game);
                picks.Add(pick);
                AddVoting(pick);
            }
        }

        public void VotingFinished(FlutistPick voting)
        {
            var result = voting.GetResultUserIds().ToArray();
            if (result.Length == 1)
            {
                foreach (var other in picks)
                    if (other != voting)
                        other.RemoveOption(result[0]);
            }
        }

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            var index = picks.FindIndex(x => x == voting);
            if (index < 0)
                return;
            var ids = picks[index].GetResultUserIds().ToArray();
            if (ids.Length > 0)
                AddVoting(picks[index] = new FlutistPick(game, ids));
            RemoveVoting(voting);
        }
    }
}
