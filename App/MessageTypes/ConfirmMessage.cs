using App.Enums;

namespace App.MessageTypes;

public class ConfirmMessage
{
    private const MessageType MessageType = Enums.MessageType.CONFIRM;

    public ushort ReferencedMessageId { get; }

    public ConfirmMessage(ushort referencedMessageId)
    {
        ReferencedMessageId = referencedMessageId;
    }

    public byte[] FormMessage()
    {
        // Allocate byte array for the message
        byte[] message = new byte[3];

        // Set message type
        message[0] = (byte)MessageType;

        // Convert ReferencedMessageId to big-endian byte array
        byte[] idBytes = BitConverter.GetBytes(ReferencedMessageId);
        Array.Reverse(idBytes); // Reverse byte order to convert to big-endian

        // Copy bytes to message array (skip first byte of idBytes)
        Array.Copy(idBytes, 0, message, 1, 2);

        return message;
    }
}