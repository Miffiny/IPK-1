using App.Enums;

namespace App.MessageTypes;

public class ServerParser
{
    private readonly ChatFSM _fsm;

    public ServerParser(ChatFSM fsm)
    {
        this._fsm = fsm;
    }
    public void ParseMessage(byte[] messageBytes)
    {
        byte messageType = messageBytes[0];

        switch (messageType)
        {
            case 0x00:  //Confirm
            case 0xFF: // ByeMessage
                _fsm.TransitionToState(ChatState.End);
                break;
            case 0x01:
                // Process ReplyMessage
                try
                {
                    var replyMessage = new ReplyMessage(messageBytes);
                    if (replyMessage.Result)
                    {
                        Console.Error.WriteLine($"Success: {replyMessage.MessageContent}");
                        if (_fsm.CurrentState == ChatState.Auth)
                        {
                            _fsm.TransitionToState(ChatState.Open);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine($"Failure: {replyMessage.MessageContent}");
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.Error.WriteLine($"ERR: {ex.Message}");
                    _fsm.TransitionToState(ChatState.Error);
                }

                break;


            case 0x04:
                // Process MsgMessage
                try
                {
                    var msgMessage = new MsgMessage(messageBytes);
                    Console.WriteLine($"{msgMessage.DisplayName}: {msgMessage.MessageContents}");
                }
                catch (ArgumentException ex)
                {
                    Console.Error.WriteLine($"ERR: {ex.Message}");
                    _fsm.TransitionToState(ChatState.Error);
                }

                break;
            case 0xFE:
                // Process ErrMessage
                try
                {
                    var errMessage = new ErrMessage(messageBytes);
                    Console.Error.WriteLine($"ERR FROM {errMessage.DisplayName}: {errMessage.MessageContents}");
                    
                    // Check if the current state is open or auth and change it to End
                    if (_fsm.CurrentState == ChatState.Open || _fsm.CurrentState == ChatState.Auth)
                    {
                        _fsm.TransitionToState(ChatState.End);
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.Error.WriteLine($"ERR: {ex.Message}");
                    _fsm.TransitionToState(ChatState.Error);
                }

                break;
            default:
                Console.Error.WriteLine("ERR: Unknown message received from the server");
                _fsm.TransitionToState(ChatState.Error);
                break;
        }
    }
}