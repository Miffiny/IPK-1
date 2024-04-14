using App.Enums;

namespace App.MessageTypes;

public class AuthMessage
{
    private const MessageType MessageType = Enums.MessageType.AUTH;

    public string Username { get; }
    public string DisplayName { get; }
    public string Secret { get; }

    public AuthMessage(string username, string secret, string displayName)
    {
        Username = username;
        DisplayName = displayName;
        Secret = secret;
    }

    public byte[] FormMessage(ushort messageId)
    {
        // List to hold bytes of the message
        List<byte> messageBytes = new List<byte>();

        // Add message type (0x02)
        messageBytes.Add((byte)MessageType);

        // Add message ID (big-endian)
        byte[] messageIdBytes = BitConverter.GetBytes(messageId);
        Array.Reverse(messageIdBytes); // Reverse byte order to convert to big-endian
        messageBytes.AddRange(messageIdBytes);

        // Add username and terminate with 0 byte
        messageBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(Username));
        messageBytes.Add(0);

        // Add display name and terminate with 0 byte
        messageBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(DisplayName));
        messageBytes.Add(0);

        // Add secret and terminate with 0 byte
        messageBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(Secret));
        messageBytes.Add(0);

        return messageBytes.ToArray();
    }
}