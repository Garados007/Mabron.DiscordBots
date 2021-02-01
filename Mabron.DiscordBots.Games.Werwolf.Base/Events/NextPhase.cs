using System.Linq;
using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class NextPhase : GameEvent
    {
        public Phase? Phase { get; }

        public NextPhase(Phase? phase)
            => Phase = phase;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            if (Phase == null)
                writer.WriteNull("phase");
            else
            {
                var ownRole = game.TryGetRole(user.DiscordId);

                writer.WriteStartObject("phase"); // phase
                writer.WriteString("lang-id", Phase.LanguageId);
                writer.WriteStartArray("voting"); // voting
                foreach (var voting in Phase.Votings)
                {
                    if (!Voting.CanViewVoting(game, user, ownRole, voting))
                        continue;
                    writer.WriteStartObject(); // {}
                    writer.WriteString("id", voting.Id.ToString());
                    writer.WriteString("lang-id", voting.LanguageId);
                    writer.WriteBoolean("started", voting.Started);
                    writer.WriteBoolean("can-vote", ownRole != null && voting.CanVote(ownRole));
                    writer.WriteNumber("max-voter", voting.GetVoter(game).Count());
                    if (voting.Timeout != null)
                        writer.WriteString("timeout", voting.Timeout.Value);
                    else writer.WriteNull("timeout");
                    writer.WriteStartObject("options"); // options
                    foreach (var (id, option) in voting.Options)
                    {
                        writer.WriteStartObject(id.ToString()); // id
                        writer.WriteString("name", option.Name);
                        writer.WriteStartArray("user");
                        foreach (var vuser in option.Users)
                            writer.WriteStringValue(vuser.ToString());
                        writer.WriteEndArray();
                        writer.WriteEndObject(); // id
                    }
                    writer.WriteEndObject(); // options
                    writer.WriteEndObject(); // {}
                }
                writer.WriteEndArray(); //voting
                writer.WriteEndObject(); // phase
            }
        }
    }
}