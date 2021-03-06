﻿using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public abstract class DiscussionPhase : Phase
    {
        public class DiscussionEnd : Voting
        {
            private readonly VoteOption option;
            private readonly DiscussionPhase phase;

            public DiscussionEnd(DiscussionPhase phase)
            {
                option = new VoteOption("end");
                this.phase = phase;
            }

            public override IEnumerable<(int id, VoteOption option)> Options
                => Enumerable.Repeat((0, option), 1);

            public override bool CanView(Role viewer)
                => phase.CanView(viewer);

            public override bool CanVote(Role voter)
                => phase.CanVote(voter);

            public override void Execute(GameRoom game, int id)
            {
            }
        }

        protected abstract bool CanView(Role viewer);

        protected abstract bool CanVote(Role voter);

        public override bool CanExecute(GameRoom game)
        {
            return game.Participants.Values.Any(x => x != null && CanVote(x));
        }

        public override void Init(GameRoom game)
        {
            base.Init(game);
            AddVoting(new DiscussionEnd(this));
        }
    }
}
