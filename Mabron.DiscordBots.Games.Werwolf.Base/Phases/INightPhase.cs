namespace Mabron.DiscordBots.Games.Werwolf.Phases
{
    public interface INightPhase<T>
        where T : Phase, INightPhase<T>
    {
    }
}
