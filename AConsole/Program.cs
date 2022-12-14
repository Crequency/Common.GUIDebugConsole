using System;
using System.IO.Pipes;
using System.Security.Principal;

namespace Common.GUIDebugConsole.AConsole;

public static class Program
{
    private static bool _isRunning = true;
    private static string _pipeName = string.Empty;
    private static NamedPipeClientStream? _pipe = null;

    public static int Main(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--connect":
                    if (i == args.Length - 1)
                        throw new InvalidDataException("Argument lost for --connect");
                    ++i;
                    _pipeName = args[i];
                    Connect();
                    break;
            }
        }

        while (_isRunning)
        {
            Console.Write(">>> ");
            Execute(Console.ReadLine());
        }

        return 0;
    }

    private static void Execute(string? cmd)
    {
        if (cmd == null)
        {
            Error("Console.ReadLine() return null value.");
            return;
        }

        switch (cmd)
        {
            case "exit":
                _isRunning = false;
                break;
            case "":
                break;
            default:
                SendData(cmd);
                break;
        }
    }

    private static void Connect()
    {
        try
        {
            _pipe = new NamedPipeClientStream("localhost", _pipeName, PipeDirection.InOut);
            _pipe?.Connect();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    private static void SendData(string msg)
    {
        try
        {
            if (_pipe == null) return;
            using var sw = new StreamWriter(_pipe);
            sw.WriteLine(msg);
            sw.Flush();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    private static void ReadData()
    {
    }

    private static void BeginReadData()
    {
        try
        {
            new Thread(() =>
            {
                while (_isRunning)
                {

                }
            }).Start();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    private static void Error(string msg) =>
        ActInColor(() => Console.WriteLine($"err {msg}"), ConsoleColor.Red);

    private static void Warning(string msg) =>
        ActInColor(() => Console.WriteLine($"war {msg}"), ConsoleColor.Yellow);

    private static void ActInColor(Action action, ConsoleColor cc)
    {
        var now = Console.ForegroundColor;
        Console.ForegroundColor = cc;
        action();
        Console.ForegroundColor = now;
    }
}