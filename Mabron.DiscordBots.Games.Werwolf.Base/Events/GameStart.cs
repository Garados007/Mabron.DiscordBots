using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class GameStart : GameEvent
    {
        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            var ownRole = game.TryGetRole(user.Id);
            writer.WriteStartObject("participants");
            foreach (var participant in game.Participants.ToArray())
            {
                if (participant.Value == null)
                    writer.WriteNull(participant.Key.ToString());
                else
                {
                    var seenRole = Role.GetSeenRole(game, null, user,
                        participant.Key, participant.Value);

                    writer.WriteStartObject(participant.Key.ToString());
                    writer.WriteStartArray("tags");
                    foreach (var tag in Role.GetSeenTags(game, user, ownRole, participant.Value))
                        writer.WriteStringValue(tag);
                    writer.WriteEndArray();
                    writer.WriteString("role", seenRole?.GetType().Name);
                    writer.WriteEndObject();
                }
            }
            writer.WriteEndObject();
        }
    }
}
