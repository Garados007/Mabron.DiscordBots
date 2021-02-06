using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class AddParticipant : GameEvent
    {
        public GameUser User { get; }

        public AddParticipant(GameUser user)
            => User = user;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            var entry = User;
            writer.WriteString("id", User.Id.ToString());
            writer.WriteString("name", entry.Username);
            writer.WriteString("img", entry.Image);
            writer.WriteStartObject("stats");
            writer.WriteNumber("win-games", entry.StatsWinGames);
            writer.WriteNumber("killed", entry.StatsKilled);
            writer.WriteNumber("loose-games", entry.StatsLooseGames);
            writer.WriteNumber("leader", entry.StatsLeader);
            writer.WriteNumber("level", entry.Level);
            writer.WriteNumber("current-xp", entry.CurrentXP);
            writer.WriteNumber("max-xp", entry.LevelMaxXP);
            writer.WriteEndObject();
        }
    }
}