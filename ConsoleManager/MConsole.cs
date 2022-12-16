using System.Diagnostics;
using System.IO.Pipes;

namespace Common.GUIDebugConsole.ConsoleManager;

public class MConsole : IDisposable
{
    private readonly string? _name = string.Empty;
    private bool _isRunning = false;
    private readonly Process _process = new();
    private NamedPipeServerStream? _pipe = null;
    private readonly Queue<string> _messages = new();
    private string _pipeServerName = string.Empty;
    private Action<string>? _onReceive;

    public string? Name
    {
        get => _name;
        init
        {
            _name = value;
            _pipeServerName = $"Common.GUIDebugConsole.{value}";
        }
    }

    public bool IsRunning => _isRunning;

    public Process Process => _process;

    public MConsole()
    {
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    /// <param name="shellPath">终端程序路径</param>
    public void Start(string shellPath)
    {
        InitServer();
        ProcessStartInfo psi = new()
        {
            FileName = Path.GetFullPath(shellPath),
            UseShellExecute = true,
            CreateNoWindow = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            Arguments = $"--connect {_pipeServerName}"
        };
        _process.StartInfo = psi;
        _process.Exited += (_, _) => _isRunning = false;
        _process.Start();
        _isRunning = true;
        Connect();
    }

    /// <summary>
    /// 初始化服务器
    /// </summary>
    private void InitServer()
    {
        _pipeServerName = $"Common.GUIDebugConsole.{Name}";
        _pipe = new NamedPipeServerStream(_pipeServerName,
            PipeDirection.InOut, 1);
    }

    /// <summary>
    /// 开始建立服务器并准备连接
    /// </summary>
    private void Connect()
    {
        if (_pipe == null) return;
        _pipe.WaitForConnection();
        _pipe.ReadMode = PipeTransmissionMode.Byte;
        new Thread(() =>
        {
            try
            {
                while (_isRunning)
                {
                    if (_pipe.IsConnected && _pipe.CanRead)
                    {
                        _messages.Enqueue(WaitData());
                        _onReceive?.Invoke(_messages.Dequeue());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }).Start();
    }

    /// <summary>
    /// 等待数据
    /// </summary>
    /// <returns>得到的数据</returns>
    private string WaitData()
    {
        try
        {
            if (_pipe == null) return string.Empty;
            using var sr = new StreamReader(_pipe);
            return sr.ReadToEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// 读取一行
    /// </summary>
    /// <returns>从消息队列拿取一条</returns>
    public string? ReadLine() => _messages.Count > 0 ? _messages.Dequeue() : null;

    /// <summary>
    /// 响应数据
    /// </summary>
    /// <param name="msg">待响应消息</param>
    public void Response(string msg)
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
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 响应数据
    /// </summary>
    /// <param name="action">对接收的操作</param>
    public void Response(Action<string> action) => _onReceive = action;

    /// <summary>
    /// 设置当进程退出时的操作
    /// </summary>
    /// <param name="action">操作</param>
    public void OnProcessExit(Action? action)
    {
        new Thread(() =>
        {
            Process.WaitForExit();
            action?.Invoke();
        }).Start();
    }

    public void Dispose()
    {
        _process.Kill();
        _pipe?.Close();
        
        _process.Dispose();
        _pipe?.Dispose();
    }
}