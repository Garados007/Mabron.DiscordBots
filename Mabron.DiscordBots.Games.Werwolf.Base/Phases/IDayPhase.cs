namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public interface IDayPhase<T>
        where T : Phase, IDayPhase<T>
    {
    }
}
