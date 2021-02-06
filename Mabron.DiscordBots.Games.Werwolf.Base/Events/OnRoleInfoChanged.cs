using LiteDB;
using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class OnRoleInfoChanged : GameEvent
    {
        public Role Role { get; }

        public uint? ExecutionRound { get; }

        public ObjectId? Target { get; }

        public OnRoleInfoChanged(Role role, uint? executionRound = null, ObjectId? target = null)
            => (Role, ExecutionRound, Target) = (role, executionRound, target);

        public override bool CanSendTo(GameRoom game, GameUser user)
            => Target == null || Target == user.Id;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            var id = game.TryGetId(Role);
            var ownRole = game.TryGetRole(user.Id);
            var seenRole = id != null ?
                Role.GetSeenRole(game, ExecutionRound, user, id, Role) : null;
            writer.WriteString("id", id?.ToString());
            writer.WriteStartArray("tags");
            foreach (var tag in Role.GetSeenTags(game, user, ownRole, Role))
                    writer.WriteStringValue(tag);
            writer.WriteEndArray();
            writer.WriteString("role", seenRole?.GetType().Name);
        }
    }
}
