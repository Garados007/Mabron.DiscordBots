using Mabron.DiscordBots.Games.Werwolf.Votings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Votings
{
    public class WitchKill : PlayerVotingBase
    {
        public Roles.Witch Witch { get; }

        public WitchKill(Roles.Witch witch, GameRoom game, IEnumerable<ulong>? participants = null) 
            : base(game, participants)
        {
            Witch = witch;
        }

        protected override bool DefaultParticipantSelector(Role role)
        {
            return role.IsAlive && role is BaseRole baseRole &&
                !baseRole.IsSelectedByWerewolves;
        }

        public override string Name => "Todestrank nutzen";

        public override bool CanView(Role viewer)
        {
            return viewer is Roles.Witch;
        }

        public override bool CanVote(Role voter)
        {
            return voter == Witch && !Witch.UsedLivePotion;
        }

        public void RemoveUserOption(ulong user)
        {
            var key = options.Where(x => x.Value.id == user)
                .Select(x => (int?)x.Key)
                .FirstOrDefault();
            if (key != null)
                options.Remove(key.Value, out _);
        }

        public override void Execute(GameRoom game, ulong id, Role role)
        {
            if (role is BaseRole baseRole)
            {
                baseRole.IsAlive = false;
                Witch.UsedDeathPotion = true;
                if (game.Phase?.Current is Phases.WitchPhase phase)
                {
                    phase.VotingFinished(this, id);
                }
            }
        }
    }
}
