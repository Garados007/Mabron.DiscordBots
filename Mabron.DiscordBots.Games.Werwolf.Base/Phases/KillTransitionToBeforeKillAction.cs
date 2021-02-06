using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public class KillTransitionToBeforeKillAction : ActionPhaseBase
    {
        public override void Execute(GameRoom game)
        {
            var dict = new Dictionary<string, HashSet<ObjectId>>();
            foreach (var (id, role) in game.Participants)
                if (role != null && role.KillState == KillState.AboutToKill)
                {
                    role.ChangeToBeforeKill(game);
                    var lid = role.KillInfo?.NotificationId ?? "";
                    if (!dict.TryGetValue(lid, out HashSet<ObjectId> set))
                        dict.Add(lid, set = new HashSet<ObjectId>());
                    set.Add(id);
                }
            if (dict.Count == 1)
            {
                var first = dict.First();
                game.SendEvent(new Events.PlayerNotification(first.Key, first.Value.ToArray()));
            }
            if (dict.Count > 1)
            {
                game.SendEvent(new Events.MultiPlayerNotification(dict));
            }
        }
    }
}
