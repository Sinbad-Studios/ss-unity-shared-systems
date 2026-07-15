namespace SinbadStudios.SharedSystems.Runtime
{
    public enum SessionStatus
    {
        OPEN,
        FULL,
        PENDING,
        IN_PROGRESS,
        BREAK,
        COMPLETED,
        DISCONNECTED
    }

    public enum SessionUserEdgeStatus
    {
        WAITING,
        READY,
        IN_GAME,
        BREAK,
        DONE,
        DISCONNECTED
    }
}
