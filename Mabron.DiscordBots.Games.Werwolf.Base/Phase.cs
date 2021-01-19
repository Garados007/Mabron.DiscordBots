using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public abstract class Phase
    {
        public abstract string Name { get; }

        public abstract bool CanExecute(GameRoom game);

        public virtual void Init(GameRoom game)
        {
            votings.Clear();
            this.game = game;
        }

        public virtual bool IsGamePhase => true;

        readonly List<Voting> votings = new List<Voting>();
        public IEnumerable<Voting> Votings => votings;

        private GameRoom? game;

        protected virtual void AddVoting(Voting voting)
        {
            votings.Add(voting);
            voting.Started = game?.AutostartVotings ?? false;
            if (game?.UseVotingTimeouts ?? false)
                voting.SetTimeout(game, true);
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
