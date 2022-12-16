using Common.GUIDebugConsole.ConsoleManager;

Manager manager = new();
manager.RegisterConsole("test");
var test = manager.GetConsole("test");
test.Start("./AConsole.exe");
test.Response(msg =>
{
    test.Response(msg);
});
bool run = true;
test.OnProcessExit(() => run = false);
while (run)
{
    Console.WriteLine(test.ReadLine() ?? "null");
}
