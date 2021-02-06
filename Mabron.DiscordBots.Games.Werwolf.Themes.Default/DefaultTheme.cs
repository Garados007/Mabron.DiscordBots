using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default
{
    public class DefaultTheme : Theme
    {
        public DefaultTheme(GameRoom? game) : base(game)
        {
        }

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
            yield return new Roles.OldMan(this);
            yield return new Roles.ScapeGoat(this);
            yield return new Roles.Flutist(this);
        }

        public override PhaseFlow GetPhases()
        {
            // init stages
            var nightStage = new Stages.NightStage();
            var morningStage = new Stages.MorningStage();
            var dayStage = new Stages.DayStage();
            var afternoonStage = new Stages.AfternoonStage();

            // build phases
            var phases = new PhaseFlowBuilder();
            static IEnumerable<Phase> KillHandling()
            {
                // remove flags if possible
                yield return new Phases.KillFlagWerwolfVictimAction();
                // transition and execute special actions
                yield return new Werwolf.Phases.KillTransitionToAboutToKillAction();
                // transition to prepare special phases
                yield return new Werwolf.Phases.KillTransitionToBeforeKillAction();
                // special phases
                yield return new Phases.HunterPhase();
                yield return new Phases.ScapeGoatPhase();
                yield return new Phases.InheritMajorPhase();
                // lastly kill and check for game end
                yield return new Werwolf.Phases.KillTransitionToKilledAction();
                yield return new Werwolf.Phases.CheckWinConditionAction();
            }

            // add init phases
            phases.Add(nightStage, true);
            phases.Add(new Phases.AmorPhase(), true);

            // add night phases
            phases.Add(nightStage);
            phases.Add(new Phase[]
            {
                new Phases.HealerPhase(),
                new Phases.OraclePhase(),
                new Phases.WerwolfPhase(),
                new Phases.WitchPhase(),
                new Phases.FlutistPhase(),
            });

            // add kill handling
            phases.Add(morningStage);
            phases.Add(KillHandling);

            // add day phases
            phases.Add(dayStage);
            phases.Add(new Phase[]
            {
                new Phases.ElectMajorPhase(),
                new Phases.DailyVictimElectionPhase(),
            });

            // add kill handling
            phases.Add(afternoonStage);
            phases.Add(new Phase[]
            {
                new Phases.ScapeGoatResetAction(),
            });
            phases.Add(KillHandling);

            return phases.Build() ?? throw new InvalidOperationException();
        }

        public override IEnumerable<WinConditionCheck> GetWinConditions()
        {
            yield return OnlyLovedOnes;
            yield return OnlyEnchanted;
        }

        static bool OnlyLovedOnes(GameRoom game, [NotNullWhen(true)] out ReadOnlyMemory<Role>? winner)
        {
            foreach (var player in game.NotKilledRoles)
                if (player is BaseRole baseRole && !baseRole.IsLoved)
                {
                    winner = null;
                    return false;
                }
            winner = game.AliveRoles.ToArray();
            return true;
        }

        static bool OnlyEnchanted(GameRoom game, [NotNullWhen(true)] out ReadOnlyMemory<Role>? winner)
        {
            foreach (var player in game.NotKilledRoles)
                if (player is BaseRole baseRole && !(baseRole.IsEnchantedByFlutist || player is Roles.Flutist))
                {
                    winner = null;
                    return false;
                }
            winner = game.Participants.Values
                .Where(x => x is Roles.Flutist).Cast<Role>().ToArray();
            return true;
        }
    }
}
