using System;
using System.IO.Pipes;
using System.Security.Principal;

namespace Common.GUIDebugConsole.AConsole;

public static class Program
{
    private static bool _isRunning = true; //  是否正在运行
    private static string _pipeName = string.Empty; //  汇报管道名称
    private static NamedPipeClientStream? _pipe = null; //  管道

    public static int Main(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                //  获取待连接的管道名称
                case "--connect":
                    if (i == args.Length - 1)
                        throw new InvalidDataException("Argument lost for --connect");
                    ++i;
                    _pipeName = args[i];
                    Connect(); //  连接管道
                    break;
            }
        }

        while (_isRunning)
        {
            Console.Write(">>> "); //  前导符
            Execute(Console.ReadLine()); //  执行输入
        }

        return 0;
    }

    private static void Execute(string? cmd)
    {
        //  没有得到输入
        if (cmd == null)
        {
            Error("Console.ReadLine() return null value.");
            return;
        }

        switch (cmd)
        {
            case "exit": //  退出命令
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
            //  初始化连接
            _pipe = new NamedPipeClientStream("127.0.0.1", _pipeName, PipeDirection.InOut);
            _pipe.Connect(5);
            if (!_pipe.IsConnected) Warning("Pipe connection broken.");
            new Thread(() =>
            {
                try
                {
                    while (_isRunning)
                        if (_pipe.IsConnected && _pipe.CanRead)
                            ReadData();
                }
                catch (Exception ex)
                {
                    Warning(ex.Message);
                }
            }).Start();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="msg">要发送的消息</param>
    private static void SendData(string msg)
    {
        try
        {
            if (_pipe == null) return; //  管道未连接则直接返回
            using var sw = new StreamWriter(_pipe);
            sw.WriteLine(msg); //  写入消息
            sw.Flush(); //  刷新缓存
            // ReadData(); //  读取远程响应
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    private static void ReadData()
    {
        try
        {
            if (_pipe == null) return;
            using var sr = new StreamReader(_pipe);
            if (_pipe.CanRead)
                Console.WriteLine(sr.ReadLine());
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