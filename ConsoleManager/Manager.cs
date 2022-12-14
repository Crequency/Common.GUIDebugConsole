namespace Common.ConsoleManager;

public class Manager
{
    private readonly Dictionary<string, MConsole> _mConsoles = new();

    public Manager()
    {
    }

    public MConsole RegisterConsole(string name)
    {
        if (!_mConsoles.ContainsKey(name))
        {
            MConsole con = new()
            {
                Name = name
            };
            _mConsoles.Add(name, con);
            return con;
        }
        else throw ManagerExceptions.OnExistsConsoleException(name);
    }

    public bool Exists(string name) => _mConsoles.ContainsKey(name);

    public MConsole GetConsole(string name)
    {
        if (_mConsoles.ContainsKey(name)) return _mConsoles[name];
        else throw ManagerExceptions.OnNoConsoleException(name);
    }

    public void RemoveConsole(string name)
    {
        if (_mConsoles.ContainsKey(name))
            _mConsoles.Remove(name);
        else throw ManagerExceptions.OnNoConsoleException(name);
    }
}

public static class ManagerExceptions
{
    public static Exception OnExistsConsoleException(string name) =>
        new InvalidDataException($"This Console already exists. Name: {name}");

    public static Exception OnNoConsoleException(string name) =>
        new InvalidDataException($"No this Console. Name: {name}");
}