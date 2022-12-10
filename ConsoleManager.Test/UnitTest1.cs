namespace ConsoleManager.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        Manager manager = new();
        manager.RegisterConsole("test");
        var test = manager.GetConsole("test");
        test.Start();
        while (test.IsRunning)
        {
            Console.WriteLine(test.ReadLine());
        }
    }
}