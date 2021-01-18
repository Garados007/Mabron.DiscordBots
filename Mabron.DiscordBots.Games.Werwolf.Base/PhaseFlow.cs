namespace Mabron.DiscordBots.Games.Werwolf
{
    public sealed class PhaseFlow
    {
        public sealed class Step
        {
            public Phase Phase { get; }

            public Step? Next { get; internal set; }

            internal bool IsSequenceFirst { get; }

            internal Step(Phase phase, bool isSequenceFirst)
                => (Phase, IsSequenceFirst) = (phase, isSequenceFirst);
        }

        public Step InitialStep { get; }

        public Step CurrentStep { get; private set; }

        public Phase Current => CurrentStep.Phase;

        internal PhaseFlow(Step step)
            => InitialStep = CurrentStep = step;

        private bool Next()
        {
            var next = CurrentStep.Next;
            if (next == null)
                return false;
            CurrentStep = next;
            return true;
        }

        public bool Next(GameRoom game)
        {
            // if this counter is larger than 2 it means there is no valid phase left.
            int firstCounter = 0;
            while (Next())
            {
                if (CurrentStep.IsSequenceFirst)
                    firstCounter++;
                if (firstCounter > 2)
                    return false;
                if (!Current.CanExecute(game))
                    continue;
                
                if (!Current.IsGamePhase)
                {
                    Current.Init(game);
                }
                else return true;
            }
            return false;
        }
    }
}