using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class PhaseFlowBuilder
    {
        readonly List<Phase> initOnlyPhases = new List<Phase>();
        readonly List<Phase> consecutivePhases = new List<Phase>();

        public void Add(Phase phase, bool initOnly = false)
        {
            var list = initOnly ? initOnlyPhases : consecutivePhases;
            list.Add(phase);
        }

        public void Add(IEnumerable<Phase> phases, bool initOnly = false)
        {
            var list = initOnly ? initOnlyPhases : consecutivePhases;
            list.AddRange(phases);
        }

        public PhaseFlow? Build()
        {
            if (initOnlyPhases.Count + consecutivePhases.Count == 0)
                return null;
            
            PhaseFlow.Step? firstConsecutive = null;
            PhaseFlow.Step? last = null;
            foreach (var phase in consecutivePhases)
            {
                var step = new PhaseFlow.Step(phase, firstConsecutive == null);
                if (last != null)
                    last.Next = step;
                firstConsecutive ??= step;
                last = step;
            }
            if (last != null)
                last.Next = firstConsecutive;
            
            PhaseFlow.Step? firstInit = null;
            last = null;
            foreach (var phase in initOnlyPhases)
            {
                var step = new PhaseFlow.Step(phase, firstInit == null);
                if (last != null)
                    last.Next = step;
                firstInit ??= step;
                last = step;
            }
            if (last != null)
                last.Next = firstConsecutive;
            
            var init = firstInit ?? firstConsecutive;
            if (init != null)
                return new PhaseFlow(init);
            else return null;
        }
    }
}