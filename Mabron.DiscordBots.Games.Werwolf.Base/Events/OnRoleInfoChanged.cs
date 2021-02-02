using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class OnRoleInfoChanged : GameEvent
    {
        public Role Role { get; }

        public OnRoleInfoChanged(Role role)
            => Role = role;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            var id = GetUserId(game);
            var seenRole = id != null ?
                Role.GetSeenRole(game, null, user, id.Value, Role) : null;
            writer.WriteString("id", id?.ToString());
            writer.WriteStartArray("tags");
            foreach (var tag in Role.GetTags(game, game.TryGetRole(user.DiscordId)))
                writer.WriteStringValue(tag);
            writer.WriteEndArray();
            writer.WriteString("role", seenRole?.GetType().Name);
        }

        private ulong? GetUserId(GameRoom game)
        {
            foreach (var (id, role) in game.Participants)
                if (Role == role)
                    return id;
            return null;
        }
    }
}
