using LiteDB;
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
            readonly HashSet<Role>? allowedVoter = null;

            public DailyVote(GameRoom game, IEnumerable<ObjectId>? participants = null) 
                : base(game, participants)
            {
                // check if scape goat phase
                var isScapeGoatRevenge = game.Participants.Values
                    .Where(x => x is Roles.ScapeGoat)
                    .Cast<Roles.ScapeGoat>()
                    .Where(x => x.HasDecided && !x.HasRevenge)
                    .Any();
                if (isScapeGoatRevenge)
                    allowedVoter = new HashSet<Role>(game.Participants.Values
                        .Where(x => x != null && x.IsAlive)
                        .Where(x => x is BaseRole baseRole && baseRole.HasVotePermitFromScapeGoat)
                        .Cast<Role>()
                    );
            }

            public override bool CanView(Role viewer)
            {
                return true;
            }

            public override bool CanVote(Role voter)
            {
                // special voting condition
                if (allowedVoter != null)
                    return allowedVoter.Contains(voter);
                // normal vote
                return voter.IsAlive && (!(voter is Roles.Idiot idiot) || !idiot.IsRevealed);
            }

            public override void Execute(GameRoom game, ObjectId id, Role role)
            {
                if (role is Roles.Idiot idiot)
                {
                    idiot.IsRevealed = true;
                    idiot.WasMajor = idiot.IsMajor;
                    idiot.IsMajor = false;
                    var oldManKilled = game.Participants.Values
                        .Where(x => x is Roles.OldMan oldMan && !oldMan.IsAlive)
                        .Any();
                    if (oldManKilled)
                    {
                        idiot.IsRevealed = false;
                        idiot.SetKill(game, new KillInfos.VillageKill());
                    }
                    return;
                }
                if (role is Roles.OldMan oldMan)
                {
                    oldMan.WasKilledByVillager = true;
                }
                role.SetKill(game, new KillInfos.VillageKill());
            }
        }

        public class MajorPick : PlayerVotingBase
        {
            public MajorPick(GameRoom game, IEnumerable<ObjectId>? participants = null) 
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

            public override void Execute(GameRoom game, ObjectId id, Role role)
            {
                role.SetKill(game, new KillInfos.KilledByMajor());
            }
        }

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
                var hasScapeGoat = game.AliveRoles.Any(x => x is Roles.ScapeGoat);
                var ids = dv.GetResultUserIds().ToArray();
                if (ids.Length > 0)
                {
                    if (hasScapeGoat)
                    {
                        foreach (var role in game.AliveRoles)
                            if (role is Roles.ScapeGoat scapeGoat)
                            {
                                // kill the scape goat and end the voting
                                scapeGoat.WasKilledByVillage = true;
                                scapeGoat.SetKill(game, new KillInfos.ScapeGoatKilled());
                            }
                    }
                    else if (hasMajor)
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
