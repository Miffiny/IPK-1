using App.Enums;

namespace App.MessageTypes;

public class MsgMessage
{
    private const MessageType MessageType = Enums.MessageType.MSG;

    public ushort MessageId { get; }
    public string DisplayName { get; }
    public string MessageContents { get; }

    public MsgMessage(ushort messageId, string displayName, string messageContents)
    {
        MessageId = messageId;
        DisplayName = displayName;
        MessageContents = messageContents;
    }

    public MsgMessage(byte[] messageBytes)
    {
        if (messageBytes == null || messageBytes.Length < 6)
        {
            throw new ArgumentException("Invalid message bytes for MsgMessage");
        }

        // Check message type
        if (messageBytes[0] != (byte)MessageType)
        {
            throw new ArgumentException("Invalid message type for MsgMessage");
        }

        // Extract message ID (big-endian)
        MessageId = (ushort)((messageBytes[1] << 8) | messageBytes[2]);

        // Extract DisplayName (string)
        DisplayName = ExtractString(messageBytes, 3, out int nextIndex);

        // Extract MessageContents (string)
        MessageContents = ExtractString(messageBytes, nextIndex, out _);
    }

    public byte[] FormMessage(ushort messageId)
    {
        // List to hold bytes of the message
        List<byte> messageBytes = new List<byte>();

        // Add message type (0x04)
        messageBytes.Add((byte)MessageType);

        // Add message ID (big-endian)
        byte[] messageIdBytes = BitConverter.GetBytes(messageId);
        Array.Reverse(messageIdBytes); // Reverse byte order to convert to big-endian
        messageBytes.AddRange(messageIdBytes);

        // Add display name and terminate with 0 byte
        messageBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(DisplayName));
        messageBytes.Add(0);

        // Add message contents and terminate with 0 byte
        messageBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(MessageContents));
        messageBytes.Add(0);

        return messageBytes.ToArray();
    }

    private string ExtractString(byte[] bytes, int startIndex, out int nextIndex)
    {
        int endIndex = Array.IndexOf(bytes, (byte)0, startIndex); // Find the index of the null terminator
        if (endIndex == -1)
        {
            throw new ArgumentException("Invalid message format: Null terminator not found");
        }

        nextIndex = endIndex + 1; // Index of the next byte after the null terminator
        return System.Text.Encoding.ASCII.GetString(bytes, startIndex, endIndex - startIndex);
    }
}