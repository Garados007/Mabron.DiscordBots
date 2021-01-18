using System;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public abstract class Phase
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
        
    }
}
