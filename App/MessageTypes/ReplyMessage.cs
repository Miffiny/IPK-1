using App.Enums;

namespace App.MessageTypes;

public class ReplyMessage
{
    private const MessageType MessageType = Enums.MessageType.REPLY;

    public ushort MessageId { get; }
    public bool Result { get; }
    public ushort RefMessageId { get; }
    public string MessageContent { get; }

    public ReplyMessage(byte[] messageBytes)
    {
        if (messageBytes == null || messageBytes.Length < 6)
        {
            throw new ArgumentException("Invalid message bytes for ReplyMessage");
        }

        // Check message type
        if (messageBytes[0] != (byte)MessageType)
        {
            throw new ArgumentException("Invalid message type for ReplyMessage");
        }

        // Extract message ID (big-endian)
        MessageId = (ushort)((messageBytes[1] << 8) | messageBytes[2]);

        // Extract Result (1 or 0)
        Result = messageBytes[3] == 1;

        // Extract RefMessageId (big-endian)
        RefMessageId = (ushort)((messageBytes[4] << 8) | messageBytes[5]);

        // Extract MessageContent (string)
        int contentLength = messageBytes.Length - 6; // Length of message content
        MessageContent = System.Text.Encoding.ASCII.GetString(messageBytes, 6, contentLength);
    }
}