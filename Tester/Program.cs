using Common.ConsoleManager;

Manager manager = new();
manager.RegisterConsole("test");
var test = manager.GetConsole("test");
test.Start("./AConsole.exe");
test.Process.WaitForExit();
