using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class SetVotingTimeout : GameEvent
    {
        public Voting Voting { get; }

        public SetVotingTimeout(Voting voting)
            => Voting = voting;

        public override bool CanSendTo(GameRoom game, GameUser user)
        {
            return Voting.CanViewVoting(game, user, game.TryGetRole(user.DiscordId), Voting);
        }

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteString("id", Voting.Id.ToString());
            if (Voting.Timeout == null)
                writer.WriteNull("timeout");
            else writer.WriteString("timeout", Voting.Timeout.Value);
        }
    }
}
