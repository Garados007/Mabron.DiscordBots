using System;
using System.Collections.Generic;

namespace Mabron.DiscordBots.Games.Werwolf.Themes.Default
{
    public class DefaultTheme : Theme
    {
        public override Role GetBasicRole()
            => new Roles.Villager(this);

        public override IEnumerable<Role> GetRoleTemplates()
        {
            yield return new Roles.Villager(this);
            yield return new Roles.Hunter(this);
            yield return new Roles.Werwolf(this);
            yield return new Roles.Oracle(this);
            yield return new Roles.Girl(this);
            yield return new Roles.Amor(this);
        }

        public override IEnumerable<Phase> GetPhases()
        {
            yield return new Phases.AmorPick();

            yield return new Phases.OraclePick();
            yield return new Phases.WerwolfPhase();

            yield return new Phases.KillWerwolfVictim();
            yield return new Phases.HunterKill();
            yield return new Phases.InheritMajor();

            yield return new Phases.ElectMajor();
            yield return new Phases.DailyVictimElection();

            yield return new Phases.HunterKill();
            yield return new Phases.InheritMajor();
        }

        public override IEnumerable<Func<GameRoom, bool>> GetWinConditions()
        {
            static bool OnlyLovedOnes(GameRoom game)
            {
                foreach (var player in game.AliveRoles)
                    if (player is BaseRole baseRole && !baseRole.IsLoved)
                        return false;
                return true;
            }

            yield return OnlyLovedOnes;
        }
    }
}
