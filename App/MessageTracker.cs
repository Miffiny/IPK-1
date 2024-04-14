using System.Net.Sockets;
using App.Enums;

namespace App
{
    public class MessageTracker(UdpClient client, ChatFSM fsm, TimeSpan resendTimeout, int maxResendAttempts)
    {
        private class MessageInfo(byte[] messageBytes)
        {
            public byte[] MessageBytes { get; } = messageBytes;
            public int ResendCount { get; set; }
            public DateTime LastSendTime { get; set; } = DateTime.UtcNow;
        }

        private readonly object _lockObj = new object();
        private readonly Dictionary<int, MessageInfo> _messages = new Dictionary<int, MessageInfo>();

        public void AddMessage(int messageId, byte[] messageBytes)
        {
            lock (_lockObj)
            {
                _messages.Add(messageId, new MessageInfo(messageBytes));
            }
        }

        public void ResendMessagesIfNecessary()
        {
            lock (_lockObj)
            {
                foreach (var kvp in _messages)
                {
                    int messageId = kvp.Key;
                    MessageInfo messageInfo = kvp.Value;

                    TimeSpan timeSinceLastSend = DateTime.UtcNow - messageInfo.LastSendTime;

                    // Resend the message if more than the resend timeout has passed since the last send
                    if (timeSinceLastSend >= resendTimeout)
                    {
                        if (messageInfo.ResendCount >= maxResendAttempts)
                        {
                            // Transition to error state if maximum resend attempts reached
                            fsm.TransitionToState(ChatState.Error);
                            Console.Error.WriteLine($"Error: Maximum resend attempts reached for message {messageId}");
                        }
                        else
                        {
                            // Resend the message
                            client.Send(messageInfo.MessageBytes, messageInfo.MessageBytes.Length);
                            messageInfo.ResendCount++;
                            messageInfo.LastSendTime = DateTime.UtcNow;
                        }
                    }
                }
            }
        }

        public void RemoveMessage(int messageId)
        {
            lock (_lockObj)
            {
                _messages.Remove(messageId);
            }
        }
    }
}
