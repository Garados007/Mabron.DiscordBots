using LiteDB;
using System.Text.Json;

namespace Mabron.DiscordBots.Games.Werwolf.Events
{
    public class OnLeaderChanged : GameEvent
    {
        public ObjectId Leader { get; }

        public OnLeaderChanged(ObjectId leader)
            => Leader = leader;

        public override bool CanSendTo(GameRoom game, GameUser user)
            => true;

        public override void WriteContent(Utf8JsonWriter writer, GameRoom game, GameUser user)
        {
            writer.WriteString("leader", Leader.ToString());
        }
    }
}