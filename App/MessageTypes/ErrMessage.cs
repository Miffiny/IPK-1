using App.Enums;

namespace App.MessageTypes;

public class ErrMessage
{
    private const MessageType MessageType = Enums.MessageType.ERR;

    public ushort MessageId { get; }
    public string DisplayName { get; }
    public string MessageContents { get; }

    public ErrMessage(byte[] messageBytes)
    {
        if (messageBytes == null || messageBytes.Length < 5)
        {
            throw new ArgumentException("ERR: Invalid message bytes for ErrMessage");
        }

        // Check message type
        if (messageBytes[0] != (byte)MessageType)
        {
            throw new ArgumentException("ERR: Invalid message type for ErrMessage");
        }

        // Extract message ID (big-endian)
        MessageId = (ushort)((messageBytes[1] << 8) | messageBytes[2]);

        // Extract DisplayName (string)
        DisplayName = ExtractString(messageBytes, 3, out int nextIndex);

        // Extract MessageContents (string)
        MessageContents = ExtractString(messageBytes, nextIndex, out _);
    }

    private string ExtractString(byte[] bytes, int startIndex, out int nextIndex)
    {
        int endIndex = Array.IndexOf(bytes, (byte)0, startIndex); // Find the index of the null terminator
        if (endIndex == -1)
        {
            throw new ArgumentException("ERR: Invalid message format: Null terminator not found");
        }

        nextIndex = endIndex + 1; // Index of the next byte after the null terminator
        return System.Text.Encoding.ASCII.GetString(bytes, startIndex, endIndex - startIndex);
    }
}