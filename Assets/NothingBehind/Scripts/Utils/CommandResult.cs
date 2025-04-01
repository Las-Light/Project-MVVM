namespace NothingBehind.Scripts.Utils
{
    public readonly struct CommandResult
    {
        public readonly int EntityId;
        public readonly bool Success;

        public CommandResult(int entityId, bool success)
        {
            EntityId = entityId;
            Success = success;
        }

        public CommandResult(bool success) : this(0, success)
        {
            Success = success;
        }
    }
}