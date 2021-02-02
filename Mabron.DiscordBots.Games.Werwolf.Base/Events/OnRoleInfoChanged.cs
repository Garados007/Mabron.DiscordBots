using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class OnRoleInfoChanged : GameEvent
    {
        public Role Role { get; }

        public uint? ExecutionRound { get; }

        public OnRoleInfoChanged(Role role, uint? executionRound = null)
            => (Role, ExecutionRound) = (role, executionRound);

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            var id = game.TryGetId(Role);
            var ownRole = game.TryGetRole(user.DiscordId);
            var seenRole = id != null ?
                Role.GetSeenRole(game, ExecutionRound, user, id.Value, Role) : null;
            writer.WriteString("id", id?.ToString());
            writer.WriteStartArray("tags");
            foreach (var tag in Role.GetSeenTags(game, user, ownRole, Role))
                    writer.WriteStringValue(tag);
            writer.WriteEndArray();
            writer.WriteString("role", seenRole?.GetType().Name);
        }
    }
}
