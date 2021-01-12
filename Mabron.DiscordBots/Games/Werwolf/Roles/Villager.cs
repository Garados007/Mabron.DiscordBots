namespace Mabron.DiscordBots.Games.Werwolf.Roles
{
    public class Villager : VillagerBase
    {
        public override string Name => "Dorfbewohner";

        public override string Description => "Einfacher Dorfbewohner ohne besondere Rolle";

        public override Role CreateNew()
        {
            return new Villager();
        }
    }
}
