using System.Net.Sockets;
using System.Text;
using App.Enums;
using App.MessageTypes;

namespace App;

public class Tcp
{
    private readonly ChatFSM _fsm;
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly TCPUserParser _userParser;
    private readonly TCPServerParser _serverParser;

    public Tcp(string serverAddress, int serverPort)
    {
        _fsm = new ChatFSM();
        _client = new TcpClient(serverAddress, serverPort);
        var stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.ASCII);
        _writer = new StreamWriter(stream, Encoding.ASCII);
        _userParser = new TCPUserParser(_fsm);
        _serverParser = new TCPServerParser(_fsm);
    }

    private async Task HandleUserInput()
    {
        while (_fsm.CurrentState != ChatState.End)
        {
            string? userInput = Console.ReadLine();
            if (userInput != null)
            {
                string? formattedMessage = _userParser.ProcessInput(userInput);

                if (!string.IsNullOrEmpty(formattedMessage))
                {
                    await _writer.WriteAsync(formattedMessage);
                    await _writer.FlushAsync();
                }
            }
        }
    }

    private async Task HandleServerMessages()
    {
        while (_fsm.CurrentState != ChatState.End)
        {
            string? serverMessage = await _reader.ReadLineAsync();
            _serverParser.ParseServerMessage(serverMessage);
        }
    }

    public async Task Run()
    {
        // Register event handler for user interrupt signal
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            _fsm.TransitionToState(ChatState.End);
        };

        Task serverTask = HandleServerMessages();
        Task userTask = HandleUserInput();


        await Task.WhenAny(userTask, serverTask);

        // Check if the current state is End
        if (_fsm.CurrentState == ChatState.End)
        {
            // Form and send BYE message to the server
            string byeMessage = "BYE\r\n";
            await _writer.WriteAsync(byeMessage);
            await _writer.FlushAsync();
        }

        _client.Close();
    }
}