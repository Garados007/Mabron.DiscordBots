namespace Mabron.DiscordBots.Games.Werwolf
{
    public abstract class Role
    {
        public bool IsAlive { get; set; } = true;

        public virtual void Reset()
        {

        }

        public abstract bool? IsSameFaction(Role other);
    }
}