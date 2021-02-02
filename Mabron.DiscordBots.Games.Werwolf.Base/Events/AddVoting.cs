using System.Linq;
using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class AddVoting : GameEvent
    {
        public Voting Voting { get; }

        public AddVoting(Voting voting)
            => Voting = voting;

        public override bool CanSendTo(GameRoom game, GameUser user)
        {
            return Voting.CanViewVoting(game, user, game.TryGetRole(user.DiscordId), Voting);
        }

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WritePropertyName("voting");
            Voting.WriteToJson(writer, game, user);
        }
    }
}
