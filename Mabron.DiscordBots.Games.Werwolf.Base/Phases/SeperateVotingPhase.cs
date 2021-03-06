﻿using LiteDB;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public abstract class SeperateVotingPhase<TVoting, TRole> : SeperateVotingPhaseBase<TVoting, TRole>
        where TVoting : PlayerVotingBase
        where TRole : Role
    {
        protected sealed override TVoting Create(TRole role, GameRoom game)
            => Create(role, game, null);

        protected abstract TVoting Create(TRole role, GameRoom game, IEnumerable<ObjectId>? ids = null);

        // this should never be used but here is the code
        public override void ExecuteMultipleWinner(Voting voting, GameRoom game)
        {
            if (voting is TVoting tvoting)
            {
                var ids = tvoting.GetResultUserIds().ToArray();
                if (ids.Length > 2)
                    AddVoting(Create(GetRole(tvoting), game, ids));
                else if (ids.Length == 1 && game.Participants.TryGetValue(ids[0], out Role? role) && role != null)
                    tvoting.Execute(game, ids[0], role);
                RemoveVoting(tvoting);
            }
        }

        public override void RemoveVoting(Voting voting)
        {
            base.RemoveVoting(voting);
            if (voting is TVoting tvoting)
            {
                var result = tvoting.GetResultUserIds().ToArray();
                if (result.Length == 1)
                    foreach (var other in Votings)
                        if (other is TVoting otherVoting)
                            otherVoting.RemoveOption(result[0]);
            }
        }
    }
}
