using LiteDB;
using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class SetVotingVote : GameEvent
    {
        public SetVotingVote(Voting voting, int option, ObjectId voter)
        {
            Voting = voting;
            Option = option;
            Voter = voter;
        }

        public Voting Voting { get; }

        public int Option { get; }

        public ObjectId Voter { get; }

        public override bool CanSendTo(GameRoom game, GameUser user)
        {
            return Voting.CanViewVoting(game, user, game.TryGetRole(user.Id), Voting);
        }

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteString("voting", Voting.Id.ToString());
            writer.WriteString("option", Option.ToString());
            writer.WriteString("voter", Voter.ToString());
        }
    }
}
