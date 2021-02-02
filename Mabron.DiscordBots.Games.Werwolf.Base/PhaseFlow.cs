using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public sealed class PhaseFlow
    {
        /// <summary>
        /// this phase is meant to be skipped durring startup
        /// </summary>
        private sealed class InitialPhase : Phase
        {
            public override bool IsGamePhase => false;

            public override bool CanExecute(GameRoom game)
            {
                return true;
            }
        }

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
            => InitialStep = CurrentStep = new Step(new InitialPhase(), false)
            {
                Next = step
            };

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

                if (Current.IsGamePhase)
                    game.SendEvent(new Events.NextPhase(Current));

                Current.Init(game);

                if (game.Phase == null)
                    return false;

                if (!Current.IsGamePhase)
                    continue;

                if (game.AutoFinishRounds && !Current.Votings.Any())
                    continue;

                return true;
            }
            return false;
        }
    }
}