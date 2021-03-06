﻿using LiteDB;
using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class InheritMajorPhase : SingleVotingPhase<InheritMajorPhase.InheritMajor>
    {
        public class InheritMajor : PlayerVotingBase
        {
            public InheritMajor(GameRoom game, IEnumerable<ObjectId>? participants = null) 
                : base(game, participants)
            {
            }

            protected override bool DefaultParticipantSelector(Role role)
            {
                return role.IsAlive && !role.IsMajor;
            }

            public override bool CanView(Role viewer)
            {
                return viewer.IsMajor;
            }

            public override bool CanVote(Role voter)
            {
                return voter.IsMajor && !voter.IsAlive;
            }

            public override void Execute(GameRoom game, ObjectId id, Role role)
            {
                foreach (var entry in game.Participants.Values)
                    if (entry != null)
                        entry.IsMajor = false;
                role.IsMajor = true;
                game.SendEvent(new Events.PlayerNotification(
                    "new-major",
                    new[] { id }
                ));
            }
        }

        public override bool CanExecute(GameRoom game)
        {
            return game.Participants.Values
                .Where(x => x != null && x.IsMajor && !x.IsAlive)
                .Any();
        }

        protected override InheritMajor Create(GameRoom game, IEnumerable<ObjectId>? ids = null)
            => new InheritMajor(game, ids);

        public override bool CanMessage(GameRoom game, Role role)
        {
            return true;
        }
    }
}
