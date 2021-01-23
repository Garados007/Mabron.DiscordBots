using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default
{
    public class DefaultTheme : Theme
    {
        public override Role GetBasicRole()
            => new Roles.Villager(this);

        public override IEnumerable<Role> GetRoleTemplates()
        {
            yield return new Roles.Villager(this);
            yield return new Roles.Hunter(this);
            yield return new Roles.Werwolf(this);
            yield return new Roles.Oracle(this);
            yield return new Roles.Girl(this);
            yield return new Roles.Amor(this);
            yield return new Roles.Witch(this);
            yield return new Roles.Healer(this);
            yield return new Roles.Idiot(this);
        }

        public override PhaseFlow GetPhases()
        {
            var phases = new PhaseFlowBuilder();

            // add init phases
            phases.Add(new Phases.AmorPick(), true);

            // add night phases
            phases.Add(new Phase[]
            {
                new Phases.HealerVote(),
                new Phases.OraclePick(),
                new Phases.WerwolfPhase(),
                new Phases.WitchPhase(),
            });

            // add kill handling
            phases.Add(new Phase[]
            {
                new Phases.KillWerwolfVictim(),
                new Phases.HunterKill(),
                new Phases.InheritMajor(),
            });

            // add day phases
            phases.Add(new Phase[]
            {
                new Phases.ElectMajor(),
                new Phases.DailyVictimElection(),
            });

            // add kill handling
            phases.Add(new Phase[]
            {
                new Phases.HunterKill(),
                new Phases.InheritMajor(),
            });

            return phases.Build() ?? throw new InvalidOperationException();
        }

        public override IEnumerable<WinConditionCheck> GetWinConditions()
        {
            yield return OnlyLovedOnes;
        }

        static bool OnlyLovedOnes(GameRoom game, [NotNullWhen(true)] out ReadOnlyMemory<Role>? winner)
        {
            foreach (var player in game.AliveRoles)
                if (player is BaseRole baseRole && !baseRole.IsLoved)
                {
                    winner = null;
                    return false;
                }
            winner = game.AliveRoles.ToArray();
            return true;
        }
    }
}
