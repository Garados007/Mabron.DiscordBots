using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class WitchPhase : MultiPhase<WitchPhase.WitchSafe, WitchPhase.WitchKill>, INightPhase<WitchPhase>
    {
        public class WitchSafe : SeperateVotingPhase<Votings.WitchSafe, Witch>
        {
            public override string Name => "";

            protected override Votings.WitchSafe Create(Witch role, GameRoom game, IEnumerable<ulong>? ids = null)
                => new Votings.WitchSafe(role, game, ids);

            protected override bool FilterVoter(Witch role)
                => role.IsAlive && !role.UsedLivePotion;

            protected override Witch GetRole(Votings.WitchSafe voting)
                => voting.Witch;

            public override void RemoveVoting(Voting voting)
            {
                base.RemoveVoting(voting);
            }
        }

        public class WitchKill : SeperateVotingPhase<Votings.WitchKill, Witch>
        {
            public override string Name => "";

            protected override Votings.WitchKill Create(Witch role, GameRoom game, IEnumerable<ulong>? ids = null)
                => new Votings.WitchKill(role, game);

            protected override bool FilterVoter(Witch role)
                => role.IsAlive && !role.UsedDeathPotion;

            protected override Witch GetRole(Votings.WitchKill voting)
                => voting.Witch;
        }

        public override string Name => "Braustunde der Hexe";
    }
}
