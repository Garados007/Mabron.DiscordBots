﻿using LiteDB;
using Mabron.DiscordBots.Games.Werwolf.Phases;
using Mabron.DiscordBots.Games.Werwolf.Votings;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class HealerPhase : SingleVotingPhase<HealerPhase.HealerVote>, INightPhase<HealerPhase>
    {
        public class HealerVote : PlayerVotingBase
        {
            public HealerVote(GameRoom game, IEnumerable<ObjectId>? participants = null) 
                : base(game, participants)
            {
            }

            protected override bool DefaultParticipantSelector(Role role)
                => role.IsAlive && role is BaseRole baseRole && !baseRole.IsSelectedByHealer;

            public override bool CanView(Role viewer)
                => viewer is Roles.Healer;

            public override bool CanVote(Role voter)
                => voter is Roles.Healer && voter.IsAlive;

            public override void Execute(GameRoom game, ObjectId id, Role role)
            {
                foreach (var other in game.Participants.Values)
                    if (other is BaseRole otherBase)
                        otherBase.IsSelectedByHealer = false;
                if (role is BaseRole baseRole)
                    baseRole.IsSelectedByHealer = true;
            }
        }

        public override bool CanExecute(GameRoom game)
        {
            return game.AliveRoles.Where(x => x is Roles.Healer).Any() &&
                !game.Participants.Values.Where(x => x is Roles.OldMan oldMan && oldMan.WasKilledByVillager).Any();
        }

        protected override HealerVote Create(GameRoom game, IEnumerable<ObjectId>? ids = null)
            => new HealerVote(game, ids);

        public override bool CanMessage(GameRoom game, Role role)
        {
            return role is Roles.Healer;
        }
    }
}
