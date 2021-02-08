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

            static PhaseFlow.PhaseGroup KillHandling(Stage stage)
            {
                var phases = new PhaseFlowBuilder();
                phases.Add(stage);
                // remove flags if possible
                phases.Add(new Phases.KillFlagWerwolfVictimAction());
                // transition and execute special actions
                phases.Add(new Werwolf.Phases.KillTransitionToAboutToKillAction());
                // transition to prepare special phases
                phases.Add(new Werwolf.Phases.KillTransitionToBeforeKillAction());
                // special phases
                phases.Add(new Phases.HunterPhase());
                phases.Add(new Phases.ScapeGoatPhase());
                phases.Add(new Phases.InheritMajorPhase());
                // lastly kill and check for game end
                phases.Add(new Werwolf.Phases.KillTransitionToKilledAction());
                phases.Add(new Werwolf.Phases.CheckWinConditionAction());
                return phases.BuildGroup() ?? throw new InvalidOperationException();
            }

            PhaseFlow.PhaseGroup DailyLoop(Stage night, Stage morning, Stage day, Stage afternoon)
            {
                var phases = new PhaseFlowBuilder();

                // add night phases
                phases.Add(night);
                phases.Add(new Phase[]
                {
                    new Phases.HealerPhase(),
                    new Phases.OraclePhase(),
                    new Phases.WerwolfPhase(),
                    new Phases.WitchPhase(),
                    new Phases.FlutistPhase(),
                });

                // add morning phases
                phases.Add(morning);
                phases.Add(KillHandling(morning));

                // add day phases
                phases.Add(day);
                phases.Add(new Phase[]
                {
                    new Phases.ElectMajorPhase(),
                    new Phases.DailyVictimElectionPhase(),
                });

                // add afternoon phases
                phases.Add(afternoon);
                phases.Add(KillHandling(afternoon));

                // return
                return phases.BuildGroup() ?? throw new InvalidOperationException();
            }

            // build phases
            var phases = new PhaseFlowBuilder();
            phases.Add(nightStage);
            phases.Add(new Phases.AmorPhase());

            phases.Add(DailyLoop(nightStage, morningStage, dayStage, afternoonStage));

            return phases.BuildPhaseFlow() ?? throw new InvalidOperationException();
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
