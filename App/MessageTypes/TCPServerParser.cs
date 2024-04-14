using App.Enums;

namespace App.MessageTypes;

public class TCPServerParser
{
    private readonly ChatFSM _fsm;

    public TCPServerParser(ChatFSM fsm)
    {
        _fsm = fsm;
    }

    public void ParseServerMessage(string? serverMessage)
    {
        if (serverMessage != null && serverMessage.StartsWith("ERR FROM"))
        {
            // Error message
            string[] parts = serverMessage.Split(new[] { "IS" }, StringSplitOptions.None);
            string displayName = parts[0].Substring("ERR FROM".Length).Trim();
            string messageContent = parts[1].Trim();

            // Print the error message to standard error stream (stderr)
            Console.Error.WriteLine($"ERR FROM {displayName}: {messageContent}");
            
            _fsm.TransitionToState(ChatState.Error);
        }
        else if (serverMessage != null && serverMessage.StartsWith("REPLY"))
        {
            // Reply message
            string[] parts = serverMessage.Split(new[] { "IS" }, StringSplitOptions.None);
            string replyType = parts[0].Substring("REPLY".Length).Trim();
            string messageContent = parts[1].Trim();
            
            if (replyType == "OK")
            {
                Console.Error.WriteLine($"Success: {messageContent}");
                if (_fsm.CurrentState == ChatState.Auth) _fsm.TransitionToState(ChatState.Open);
            }
            else if (replyType == "NOK")
            {
                Console.Error.WriteLine($"Failure: {messageContent}");
            }
            else
            {
                // Invalid reply type
                Console.Error.WriteLine("ERR: Invalid reply message received from the server");
            }
        }
        else if (serverMessage != null && serverMessage.StartsWith("MSG FROM"))
        {
            // Message from another user
            string[] parts = serverMessage.Split(new[] { "IS" }, StringSplitOptions.None);
            string displayName = parts[0].Substring("MSG FROM".Length).Trim();
            string messageContent = parts[1].Trim();

            // Print the message to stdout
            Console.WriteLine($"{displayName}: {messageContent}");
        }
        else if (serverMessage != null && serverMessage.StartsWith("BYE"))
        {
            // Server termination message
            Console.Error.WriteLine("Server terminated the connection");
            _fsm.TransitionToState(ChatState.End);
        }
        else
        {
            // Unexpected or invalid message
            Console.Error.WriteLine("ERR: Unexpected message received from the server");
            // Update FSM state to Error
            _fsm.TransitionToState(ChatState.Error);
        }
    }
}