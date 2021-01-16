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
            autostart = game.AutostartVotings;
        }

        public virtual bool IsGamePhase => true;

        readonly List<Voting> votings = new List<Voting>();
        public IEnumerable<Voting> Votings => votings;

        private bool autostart;

        protected virtual void AddVoting(Voting voting)
        {
            votings.Add(voting);
            voting.Started = autostart;
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
            yield return new Phases.AmorPick();

            yield return new Phases.OraclePick();
            yield return new Phases.WerwolfPhase();

            yield return new Phases.KillWerwolfVictim();
            yield return new Phases.HunterKill();
            yield return new Phases.InheritMajor();

            yield return new Phases.ElectMajor();
            yield return new Phases.DailyVictimElection();

            yield return new Phases.HunterKill();
            yield return new Phases.InheritMajor();
        }

        public static Phase? GetNextPhase(GameRoom game)
        {
            bool currentPhaseFound = false;
            for (int i = 0; i < 2; ++i)
                foreach (var phase in GetPhases())
                {
                    if (phase == game.Phase)
                    {
                        currentPhaseFound = true;
                        continue;
                    }
                    if (!phase.CanExecute(game))
                        continue;
                    if (game.Phase == null || currentPhaseFound)
                    {
                        if (!phase.IsGamePhase)
                        {
                            // this phase executes some stuff
                            phase.Init(game); // execute their routine
                        }
                        else return phase;
                    }
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
