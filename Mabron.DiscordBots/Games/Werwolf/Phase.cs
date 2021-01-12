using System;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public abstract class Phase : IEquatable<Phase?>
    {
        public abstract string Name { get; }

        public abstract bool CanExecute(GameRoom game);

        public virtual void Init(GameRoom game)
        {
        }

        public virtual bool IsGamePhase => true;

        readonly List<Voting> votings = new List<Voting>();
        public IEnumerable<Voting> Votings => votings;


        protected virtual void AddVoting(Voting voting)
        {
            votings.Add(voting);
        }

        public virtual void RemoveVoting(Voting voting)
        {
            votings.Remove(voting);
        }

        public virtual void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {

        }

        public override bool Equals(object? obj)
        {
            if (obj is Phase phase)
                return Equals(phase);
            else return false;
        }

        public static IEnumerable<Phase> GetPhases()
        {
            yield return new Phases.WerwolfPhase();
            yield return new Phases.KillWerwolfVictim();
            yield return new Phases.ElectMajor();
            yield return new Phases.DailyVictimElection();
        }

        public static Phase? GetNextPhase(GameRoom game)
        {
            var phases = GetPhases().Union(GetPhases())
                .Where(x => x.CanExecute(game));
            bool currentPhaseFound = false;
            foreach (var phase in phases)
            {
                if (game.Phase == null || currentPhaseFound)
                {
                    if (!phase.IsGamePhase)
                    {
                        // this phase executes some stuff
                        phase.Init(game); // execute their routine
                    }
                    else return phase;
                }
                if (phase == game.Phase)
                    currentPhaseFound = true;
            }
            return null;
        }

        public bool Equals(Phase? other)
        {
            return GetType().FullName == other?.GetType().FullName;
        }

        public override int GetHashCode()
        {
            return GetType().FullName?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Phase? left, Phase? right)
        {
            return EqualityComparer<Phase>.Default.Equals(left, right);
        }

        public static bool operator !=(Phase? left, Phase? right)
        {
            return !(left == right);
        }
    }
}
