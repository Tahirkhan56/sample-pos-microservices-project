namespace Pos.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Ambient information about the message currently being consumed.</summary>
public sealed record MessageContext(string MessageId)
{
    private static readonly AsyncLocal<MessageContext?> CurrentContext = new();

    public static MessageContext? Current
    {
        get => CurrentContext.Value;
        internal set => CurrentContext.Value = value;
    }
}
