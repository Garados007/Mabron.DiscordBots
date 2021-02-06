using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class SetGameConfig : GameEvent
    {
        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteStartObject("config");
            foreach (var (role, amount) in game.RoleConfiguration.ToArray())
            {
                writer.WriteNumber(role.GetType().Name, amount);
            }
            writer.WriteEndObject();

            writer.WriteBoolean("leader-is-player", game.LeaderIsPlayer);
            writer.WriteBoolean("dead-can-see-all-roles", game.DeadCanSeeAllRoles);
            writer.WriteBoolean("all-can-see-role-of-dead", game.AllCanSeeRoleOfDead);
            writer.WriteBoolean("autostart-votings", game.AutostartVotings);
            writer.WriteBoolean("autofinish-votings", game.AutoFinishVotings);
            writer.WriteBoolean("voting-timeout", game.UseVotingTimeouts);
            writer.WriteBoolean("autofinish-rounds", game.AutoFinishRounds);
        }
    }
}
