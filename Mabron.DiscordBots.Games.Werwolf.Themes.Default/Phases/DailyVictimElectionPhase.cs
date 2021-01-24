using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class DailyVictimElectionPhase : Phase, IDayPhase<DailyVictimElectionPhase>
    {
        public class DailyVote : PlayerVotingBase
        {
            public DailyVote(GameRoom game, IEnumerable<ulong>? participants = null) 
                : base(game, participants)
            {
            }

            public override bool CanView(Role viewer)
            {
                return true;
            }

            public override bool CanVote(Role voter)
            {
                return voter.IsAlive && (!(voter is Roles.Idiot idiot) || !idiot.IsRevealed);
            }

            public override void Execute(GameRoom game, ulong id, Role role)
            {
                if (role is Roles.Idiot idiot)
                {
                    idiot.IsRevealed = true;
                    idiot.WasMajor = idiot.IsMajor;
                    idiot.IsMajor = false;
                    return;
                }
                role.Kill(game);
            }
        }

        public class MajorPick : PlayerVotingBase
        {
            public MajorPick(GameRoom game, IEnumerable<ulong>? participants = null) 
                : base(game, participants)
            {
            }

            public override bool CanView(Role viewer)
            {
                return true;
            }

            public override bool CanVote(Role voter)
            {
                return voter.IsMajor && voter.IsAlive;
            }

            public override void Execute(GameRoom game, ulong id, Role role)
            {
                role.Kill(game);
            }
        }

        public override string Name => "Tag: Dorfwahl";

        public override bool CanExecute(GameRoom game)
        {
            return true;
        }

        public override void Init(GameRoom game)
        {
            base.Init(game);
            AddVoting(new DailyVote(game));
        }

        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting is DailyVote dv)
            {
                var hasMajor = game.AliveRoles.Any(x => x is BaseRole baserRole && x.IsMajor);
                var ids = dv.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                {
                    if (hasMajor)
                        AddVoting(new MajorPick(game, ids));
                    else AddVoting(new DailyVote(game, ids));
                }
                RemoveVoting(voting);
            }
            if (voting is MajorPick mp)
            {
                var ids = mp.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                    AddVoting(new MajorPick(game, ids));
                RemoveVoting(voting);
            }
        }
    }
}
