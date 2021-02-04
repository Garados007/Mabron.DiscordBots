using System;
using System.Collections.Generic;
using OneOf;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class PhaseFlowBuilder
    {
        readonly List<OneOf<Stage, Phase>> initOnlyPhases = new List<OneOf<Stage, Phase>>();
        readonly List<OneOf<Stage, Phase>> consecutivePhases = new List<OneOf<Stage, Phase>>();

        public void Add(Phase phase, bool initOnly = false)
        {
            var list = initOnly ? initOnlyPhases : consecutivePhases;
            list.Add(phase);
        }

        public void Add(IEnumerable<Phase> phases, bool initOnly = false)
        {
            var list = initOnly ? initOnlyPhases : consecutivePhases;
            foreach (var phase in phases)
                list.Add(phase);
        }

        public void Add(Func<IEnumerable<Phase>> phases, bool initOnly = false)
        {
            Add(phases(), initOnly);
        }

        public void Add(Stage stage, bool initOnly = false)
        {
            if (initOnly)
                initOnlyPhases.Add(stage);
            else consecutivePhases.Add(stage);
        }

        public PhaseFlow? Build()
        {
            if (initOnlyPhases.Count + consecutivePhases.Count == 0)
                return null;
            
            PhaseFlow.Step? firstConsecutive = null;
            PhaseFlow.Step? last = null;
            Stage? firstC = null;
            Stage? stage;
            foreach (var stagePhase in consecutivePhases)
            {
                if (stagePhase.TryPickT0(out stage, out Phase phase))
                {
                    firstC ??= stage;
                    continue;
                }
                if (stage == null)
                    return null;
                var step = new PhaseFlow.Step(stage, phase, firstConsecutive == null);
                if (last != null)
                    last.Next = step;
                firstConsecutive ??= step;
                last = step;
            }
            if (last != null)
                last.Next = firstConsecutive;
            
            PhaseFlow.Step? firstInit = null;
            last = null;
            Stage? firstI = null;
            foreach (var stagePhase in initOnlyPhases)
            {
                if (stagePhase.TryPickT0(out stage, out Phase phase))
                {
                    firstI ??= stage;
                    continue;
                }
                if (stage == null)
                    return null;
                var step = new PhaseFlow.Step(stage, phase, firstInit == null);
                if (last != null)
                    last.Next = step;
                firstInit ??= step;
                last = step;
            }
            if (last != null)
                last.Next = firstConsecutive;
            
            var init = firstInit ?? firstConsecutive;
            stage = firstI ?? firstC;
            if (init != null && stage != null)
                return new PhaseFlow(stage, init);
            else return null;
        }
    }
}