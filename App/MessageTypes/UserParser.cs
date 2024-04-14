using App.Enums;

namespace App.MessageTypes;

public class UserParser
{   
    private readonly ChatFSM fsm;

    public UserParser(ChatFSM fsm)
    {
        this.fsm = fsm;
    }
    // Static property to store the display name
    private static string? DisplayName { get; set; }

    public byte[]? ParseUserInput(string userInput, ushort messageId)
    {
        // Split the user input by spaces
        string[] inputParts = userInput.Trim().Split(' ');

        // Check the first word of the input
        switch (inputParts[0])
        {
            case "/auth":
                // Check if the current state is not Open
                if (fsm.CurrentState == ChatState.Open)
                {
                    Console.WriteLine("You are already authenticated.");
                    return null;
                }

                // Ensure there are exactly four parts for /auth command
                if (inputParts.Length != 4)
                {
                    Console.WriteLine("Invalid /auth command. Please provide username, secret, and display name.");
                    return null;
                }

                // Extract the username, secret, and display name
                string username = inputParts[1];
                string secret = inputParts[2];
                string displayName = inputParts[3];
                DisplayName = displayName;

                try
                {
                    // Create an instance of AuthMessage and form the message
                    var authMessage = new AuthMessage(username, secret, displayName);
                    // Set the current state to Auth
                    fsm.TransitionToState(ChatState.Auth); 
                    return authMessage.FormMessage(messageId);
                }
                catch (ArgumentException ex)
                { 
                    Console.WriteLine($"ERR: Error forming AuthMessage: {ex.Message}");
                    fsm.TransitionToState(ChatState.Error);
                    return null;
                }
            case "/join":
                // Check if the current state is Open
                if (fsm.CurrentState != ChatState.Open)
                {
                    Console.WriteLine("You need to authenticate before joining a channel.");
                    return null;
                }
                // Ensure there are exactly two parts for /join command
                if (inputParts.Length != 2)
                {
                    Console.WriteLine("Invalid /join command. Please provide the channel ID.");
                    return null;
                }

                // Extract the channel ID
                string channelId = inputParts[1];

                try
                {
                    // Create an instance of JoinMessage and form the message
                    if (DisplayName != null)
                    {
                        var joinMessage = new JoinMessage(channelId, DisplayName);
                        return joinMessage.FormMessage(messageId);
                    }
                    return null;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"ERR: Error forming JoinMessage: {ex.Message}");
                    fsm.TransitionToState(ChatState.Error);
                    return null;
                }
            case "/rename":
                // Ensure there is exactly one part after /rename command
                if (inputParts.Length != 2)
                {
                    Console.WriteLine("Invalid /rename command. Please provide a new display name.");
                    return null;
                }

                // Extract the new display name
                string newDisplayName = inputParts[1];

                // Update the display name in the UserParser class
                DisplayName = newDisplayName;

                // No need to return a message, just update the display name
                return null;
            case "/help":
                // Print out the list of available commands
                Console.WriteLine("Available commands:");
                Console.WriteLine("/auth <username> <secret> <display name> - Authenticate with the server");
                Console.WriteLine("/join <channel ID> - Join a chat channel");
                Console.WriteLine("/rename <new display name> - Change your display name");
                Console.WriteLine("/help - Show this help message");
                return null;
            default:
                // Check if the current state is Open
                if (fsm.CurrentState is ChatState.Start or ChatState.Auth)
                {
                    Console.WriteLine("You need to authenticate before sending messages.");
                    return null;
                }
                if (fsm.CurrentState is ChatState.End) return null;
                // Any other message
                try
                {
                    // Create an instance of MsgMessage
                    if (DisplayName != null)
                    {
                        var msgMessage = new MsgMessage(messageId, DisplayName, userInput);
                        Console.WriteLine($"{DisplayName}: {userInput}");
                        // Call FormMessage for this instance
                        return msgMessage.FormMessage(messageId);
                    }

                    return null;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error forming MsgMessage: {ex.Message}");
                    fsm.TransitionToState(ChatState.Error);
                    return null;
                }
        }
    }
}