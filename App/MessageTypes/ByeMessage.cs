using App.Enums;

namespace App.MessageTypes;

public class ByeMessage
{
    private const MessageType MessageType = Enums.MessageType.BYE;

    public ushort MessageId { get; }

    public ByeMessage(ushort messageId)
    {
        MessageId = messageId;
    }

    public byte[] FormMessage(ushort messageId)
    {
        // List to hold bytes of the message
        List<byte> messageBytes = new List<byte>();

        // Add message type (0xFF)
        messageBytes.Add((byte)MessageType);

        // Add message ID (big-endian)
        byte[] messageIdBytes = BitConverter.GetBytes(messageId);
        Array.Reverse(messageIdBytes); // Reverse byte order to convert to big-endian
        messageBytes.AddRange(messageIdBytes);

        return messageBytes.ToArray();
    }
}