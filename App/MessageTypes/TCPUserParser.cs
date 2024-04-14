using App.Enums;

namespace App.MessageTypes;

public class TCPUserParser
{
    private readonly ChatFSM _fsm;
    public TCPUserParser(ChatFSM fsm)
    {
        _fsm = fsm;
    }
    private static string? DisplayName { get; set; }
    public string? ProcessInput(string userInput)
    {
        
        string[] inputParts = userInput.Trim().Split(' ');

        switch (inputParts[0].ToLower())
        {
            case "/auth":
                if (_fsm.CurrentState == ChatState.Open)
                {
                    Console.WriteLine("You are already authenticated.");
                    return null;
                }
                if (inputParts.Length != 4)
                {
                    Console.WriteLine("Invalid /auth command. Please provide ID, secret, and display name.");
                    return null;
                }
                string id = inputParts[1];
                string secret = inputParts[2];
                string dname = inputParts[3];
                DisplayName = dname;
                _fsm.TransitionToState(ChatState.Auth);
                return FormatAuthMessage(id, secret, dname);

            case "/join":
                if (_fsm.CurrentState != ChatState.Open)
                {
                    Console.WriteLine("You need to authenticate before joining a channel.");
                    return null;
                }
                if (inputParts.Length != 2)
                {
                    Console.WriteLine("Invalid /join command. Please provide the channel ID.");
                    return null;
                }
                string channelId = inputParts[1];
                return FormatJoinMessage(channelId);

            case "/rename":
                if (inputParts.Length != 2)
                {
                    Console.WriteLine("Invalid /rename command. Please provide a new display name.");
                    return null;
                }
                string newDisplayName = inputParts[1];

                DisplayName = newDisplayName;
                Console.WriteLine($"Display name changed to {DisplayName}.");
                return null;

            case "/help":
                Console.WriteLine("Available commands:\\n/auth id secret dname\\n/join channelid\\n/rename new_dname\\n/help");
                return null;

            default:
                if (_fsm.CurrentState is ChatState.Auth or ChatState.Start)
                {
                    Console.WriteLine("You need to authenticate before sending messages.");
                    return null;
                }
                if (_fsm.CurrentState is ChatState.End) return null;
                Console.WriteLine($"{DisplayName}: {userInput}");
                return FormatContentMessage(userInput);
        }
    }

    private string FormatAuthMessage(string id, string secret, string dname)
    {
        return $"AUTH {id} AS {dname} USING {secret}\r\n";
    }

    private string FormatJoinMessage(string channelId)
    {
        return $"JOIN {channelId} AS {DisplayName}\r\n";
    }

    private string FormatContentMessage(string messageContent)
    {
        return $"MSG FROM {DisplayName} IS {messageContent}\r\n";
    }
}
