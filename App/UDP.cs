using System.Net;
using System.Net.Sockets;
using App.Enums;
using App.MessageTypes;

namespace App;

public class Udp
{
    private readonly ChatFSM _fsm;
    private readonly ServerParser _serverParser;
    private readonly UserParser _userParser;
    private readonly UdpClient _client;
    private readonly MessageTracker _messageTracker;
    private ushort _messageId;
    private int _dynamicPort = -1;

    private readonly int _resendCount;
    private readonly TimeSpan _resendTimeout;

    private readonly string _serverIPaddress;
    private readonly int _serverPort;

    private readonly HashSet<ushort> _confirmedMessages = new HashSet<ushort>();

    public Udp(string serverIPaddress, int serverPort, int resendCount, TimeSpan resendTimeout)
    {
        _serverIPaddress = serverIPaddress;
        _serverPort = serverPort;
        _resendCount = resendCount;
        _resendTimeout = resendTimeout;

        _fsm = new ChatFSM();
        _serverParser = new ServerParser(_fsm);
        _userParser = new UserParser(_fsm);
        _client = new UdpClient();
        _messageTracker = new MessageTracker(_client, _fsm, resendTimeout, resendCount);
        _client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
    }

    private async Task ReceiveFromServerTask()
    {
        while (_fsm.CurrentState != ChatState.End)
        {
            try
            {
                UdpReceiveResult result = await _client.ReceiveAsync();
                byte[] data = result.Buffer;
                if (_dynamicPort == -1 && data[0] == 0x01)
                {
                    _dynamicPort = result.RemoteEndPoint.Port;
                    _client.Connect($"{_serverIPaddress}", _dynamicPort);
                }
                if (data[0] != 0x00)
                {
                    ushort referencedMessageId = (ushort)((data[1] << 8) | data[2]);
                    ConfirmMessage confirmMessage = new ConfirmMessage(referencedMessageId);
                    byte[] confirmMessageBytes = confirmMessage.FormMessage();
                    SendMessageToServer(confirmMessageBytes);
                    if (_confirmedMessages.Contains(referencedMessageId))
                    {
                        continue;
                    }
                    _serverParser.ParseMessage(data);
                    _confirmedMessages.Add(referencedMessageId);
                }
                else
                {
                    ushort referenceId = (ushort)((data[1] << 8) | data[2]);
                    _messageTracker.RemoveMessage(referenceId);
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"ERR: Error receiving data from server: {ex.Message}");
                _fsm.TransitionToState(ChatState.Error);
            }
        }
    }
    
    private async Task ReadUserInputTask()
    {
        while (_fsm.CurrentState != ChatState.End)
        {
            try
            {
                string? userInput = Console.ReadLine();
                if (userInput != null)
                {
                    byte[]? messageBytes = _userParser.ParseUserInput(userInput, _messageId);
                    if (messageBytes != null)
                    {
                        _messageTracker.AddMessage(_messageId, messageBytes);
                        SendMessageToServer(messageBytes);
                        _messageId++;
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"ERR: error reading user input: {ex.Message}");
                _fsm.TransitionToState(ChatState.Error);
            }
        }
    }
    public async Task StartAsync()
    {
        Console.CancelKeyPress += OnCancelKeyPress;
        try
        {
            Task serverTask = ReceiveFromServerTask();
            Task userTask = ReadUserInputTask();
            
            await Task.WhenAny(serverTask, userTask);

            if (_fsm.CurrentState == ChatState.End)
            {
                ByeMessage byeMessage = new ByeMessage(_messageId);
                byte[] byeMessageBytes = byeMessage.FormMessage(_messageId);
                SendMessageToServer(byeMessageBytes);
                await Task.Delay(_resendTimeout);
                uint counter = 0;
                while (counter < _resendCount)
                {
                    if (await CheckForResponse())
                    {
                        break;
                    }
                    SendMessageToServer(byeMessageBytes);
                    counter++;
                    await Task.Delay(_resendTimeout);
                }
                if (counter >= _resendCount)
                {
                    _fsm.TransitionToState(ChatState.Error);
                    await Console.Error.WriteLineAsync("ERR: Maximum resend attempts reached for ByeMessage");
                }
            }
        }
        finally
        {
            _client.Close();
        }
    }

    private async Task<bool> CheckForResponse()
    {
        try
        {
            UdpReceiveResult result = await _client.ReceiveAsync();
            byte[] data = result.Buffer;
            return data[0] == 0x00;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"ERR: Error receiving response from server: {ex.Message}");
            return false;
        }
    }

    private void SendMessageToServer(byte[] messageBytes)
    {
        try
        {
            if (_dynamicPort == -1)
                _client.Send(messageBytes, messageBytes.Length, $"{_serverIPaddress}", _serverPort);
            else
                _client.Send(messageBytes, messageBytes.Length);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERR: Error sending message to server: {ex.Message}");
            _fsm.TransitionToState(ChatState.Error);
        }
    }

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        if (_fsm.CurrentState == ChatState.Open)
        {
            _fsm.TransitionToState(ChatState.End);
            e.Cancel = true;
        }
    }
}