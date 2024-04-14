using App.Enums;

namespace App.MessageTypes;

public class JoinMessage
{
    private const MessageType MessageType = Enums.MessageType.JOIN;

    public string ChannelId { get; }
    public string DisplayName { get; }

    public JoinMessage(string channelId, string displayName)
    {
        ChannelId = channelId;
        DisplayName = displayName;
    }

    public byte[] FormMessage(ushort messageId)
    {
        // List to hold bytes of the message
        List<byte> messageBytes = new List<byte>();

        // Add message type (0x03)
        messageBytes.Add((byte)MessageType);

        // Add message ID (big-endian)
        byte[] messageIdBytes = BitConverter.GetBytes(messageId);
        Array.Reverse(messageIdBytes); // Reverse byte order to convert to big-endian
        messageBytes.AddRange(messageIdBytes);

        // Add channel ID and terminate with 0 byte
        messageBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(ChannelId));
        messageBytes.Add(0);

        // Add display name and terminate with 0 byte
        messageBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(DisplayName));
        messageBytes.Add(0);

        return messageBytes.ToArray();
    }
}