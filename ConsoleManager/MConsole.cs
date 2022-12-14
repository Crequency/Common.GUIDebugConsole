using System.Diagnostics;
using System.IO.Pipes;

namespace Common.ConsoleManager;

public class MConsole
{
    private bool _isRunning = false;
    private readonly Process _process = new();
    private NamedPipeServerStream? _pipe = null;
    private readonly Queue<string> _messages = new();

    public string? Name { get; set; }

    public bool IsRunning => _isRunning;

    public Process Process => _process;

    public MConsole()
    {
    }

    public void Start(string shellPath)
    {
        ProcessStartInfo psi = new()
        {
            FileName = Path.GetFileName(shellPath),
            UseShellExecute = true,
            CreateNoWindow = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            Arguments = ""
        };
        _process.StartInfo = psi;
        _process.Exited += (_, _) => _isRunning = false;
        _process.Start();
        _isRunning = true;
        Connect();
    }

    private void Connect()
    {
        _pipe = new NamedPipeServerStream($"Common.GUIDebugConsole.{Name}",
            PipeDirection.InOut, 1);
        _pipe.WaitForConnection();
        _pipe.ReadMode = PipeTransmissionMode.Byte;
        new Thread(() =>
        {
            while (_isRunning)
            {
                if (_pipe.IsConnected && _pipe.CanRead)
                {
                    _messages.Enqueue(WaitData());
                }
            }
        }).Start();
    }

    private string WaitData()
    {
        try
        {
            if (_pipe == null) return string.Empty;
            using var sr = new StreamReader(_pipe);
            return sr.ReadLine() ?? string.Empty;
        }
        catch (IOException e)
        {
            return string.Empty;
        }
    }

    public string ReadLine() => _messages.Dequeue();
}