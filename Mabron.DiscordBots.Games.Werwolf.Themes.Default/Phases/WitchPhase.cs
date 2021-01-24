using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Themes.Default.Roles;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class WitchPhase : MultiPhase<WitchPhase.WitchSafePhase, WitchPhase.WitchKillPhase>, INightPhase<WitchPhase>
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

        public class WitchSafePhase : SeperateVotingPhase<WitchSafe, Witch>
        {
            public override string Name => "";

            protected override WitchSafe Create(Witch role, GameRoom game, IEnumerable<ulong>? ids = null)
                => new WitchSafe(role, game, ids);

            protected override bool FilterVoter(Witch role)
                => role.IsAlive && !role.UsedLivePotion;

            protected override Witch GetRole(WitchSafe voting)
                => voting.Witch;

            public override void RemoveVoting(Voting voting)
            {
                base.RemoveVoting(voting);
            }
        }

        public class WitchKillPhase : SeperateVotingPhase<WitchKill, Witch>
        {
            public override string Name => "";

            protected override WitchKill Create(Witch role, GameRoom game, IEnumerable<ulong>? ids = null)
                => new WitchKill(role, game);

            protected override bool FilterVoter(Witch role)
                => role.IsAlive && !role.UsedDeathPotion;

            protected override Witch GetRole(WitchKill voting)
                => voting.Witch;
        }

        public override string Name => "Braustunde der Hexe";
    }
}
