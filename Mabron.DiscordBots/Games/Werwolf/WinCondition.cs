using System;
using System.Collections.Generic;
using System.Linq;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class WinCondition
    {
        public bool Check(GameRoom game)
        {
            foreach (var condition in GetConditions())
                if (condition(game))
                    return true;
            return false;
        }

        IEnumerable<Func<GameRoom, bool>> GetConditions()
        {
            yield return OnlyOneFaction;
        }

        bool OnlyOneFaction(GameRoom game)
        {
            static bool IsSameFaction(Role role1, Role role2)
            {
                var check = role1.IsSameFaction(role2);
                if (check == null)
                    check = role2.IsSameFaction(role1);
                return check ?? false;
            }

            Span<Role> player = game.AliveRoles.ToArray();
            for (int i = 0; i < player.Length; ++i)
                for (int j = i + 1; j < player.Length; ++j)
                    if (!IsSameFaction(player[i], player[j]))
                        return false;
            return true;
        }

    }
}
