namespace App;

static class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var arguments = ParseArguments(args);

            // Check if help option is specified
            if (arguments.ContainsKey("-h"))
            {
                PrintHelp();
                return;
            }

            // Check if mandatory arguments -t and -s are provided
            if (!arguments.ContainsKey("-t") || !arguments.ContainsKey("-s"))
            {
                Console.WriteLine("Error: Both -t and -s arguments are mandatory.");
                PrintHelp();
                return;
            }

            // Get transport protocol
            string transportProtocol = arguments["-t"];

            // Get server IP address or hostname
            string serverAddress = arguments["-s"];

            // Get server port
            int serverPort = arguments.TryGetValue("-p", out var argument) ? int.Parse(argument) : 4567;

            // Initialize UDP client
            if (transportProtocol == "udp")
            {
                int resendCount = arguments.TryGetValue("-r", out var argument1) ? int.Parse(argument1) : 3;
                int resendTimeout = arguments.TryGetValue("-d", out var argument2) ? int.Parse(argument2) : 250;

                Udp udpClient = new Udp(serverAddress, serverPort, resendCount, TimeSpan.FromMilliseconds(resendTimeout));
                await udpClient.StartAsync();
            }
            // Initialize TCP client
            else if (transportProtocol == "tcp")
            {
                Tcp tcpClient = new Tcp(serverAddress, serverPort);
                await tcpClient.Run();
            }
            else Console.WriteLine("Invalid transport protocol specified. Use 'tcp' or 'udp'.");
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"An error occurred: {ex.Message}");
        }
    }


    // Helper method to parse command-line arguments
    static Dictionary<string, string> ParseArguments(string[] args)
    {
        var arguments = new Dictionary<string, string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("-"))
            {
                string key = args[i];
                string value = i + 1 < args.Length && !args[i + 1].StartsWith("-") ? args[i + 1] : "";
                arguments[key] = value;
            }
        }

        return arguments;
    }

    // Helper method to print program help output
    static void PrintHelp()
    {
        Console.WriteLine("Usage: program.exe -t <transport_protocol> -s <server_address> [-p <server_port>] [-d <udp_confirmation_timeout>] [-r <max_udp_retransmissions>] [-h]");
        Console.WriteLine("Arguments:");
        Console.WriteLine("  -t\tUser provided\ttcp or udp\tTransport protocol used for connection");
        Console.WriteLine("  -s\tUser provided\tIP address or hostname\tServer IP or hostname (mandatory)");
        Console.WriteLine("  -p\t4567\tuint16\tServer port");
        Console.WriteLine("  -d\t250\tuint16\tUDP confirmation timeout");
        Console.WriteLine("  -r\t3\tuint8\tMaximum number of UDP retransmissions");
        Console.WriteLine("  -h\t\t\tPrints program help output and exits");
    }
}