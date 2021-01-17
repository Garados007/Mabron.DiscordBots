using Mabron.DiscordBots.Games.Werwolf.Phases;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default.Phases
{
    public class InheritMajor : SingleVotingPhase<Votings.InheritMajor>
    {
        public override string Name => "Vererbung des Bürgermeisters";

        public override bool CanExecute(GameRoom game)
        {
            return game.Participants.Values
                .Where(x => x != null && x.IsMajor && !x.IsAlive)
                .Any();
        }

        protected override Votings.InheritMajor Create(GameRoom game, IEnumerable<ulong>? ids = null)
            => new Votings.InheritMajor(game, ids);
    }
}
