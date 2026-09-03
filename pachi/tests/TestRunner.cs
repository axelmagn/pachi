using System;
using System.Reflection;
using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Chickensoft.Log;
using Godot;

[GlobalClass]
public partial class TestRunner : SceneTree
{
    public sealed class TestLogWriter : ILogWriter
    {
        public bool HasErrors { get; private set; }

        public void WriteMessage(string message) => GD.Print(message);
        public void WriteWarning(string message) => GD.PushWarning(message);
        public void WriteError(string message)
        {
            HasErrors = true;
            GD.PrintErr(message);
        }
    }

    public override void _Initialize()
    {
        CallDeferred(nameof(StartTests));
    }

    private void StartTests()
    {
        _ = RunTestsAsync();
    }

    private async Task RunTestsAsync()
    {
        try
        {
            var args = OS.GetCmdlineArgs();
            var userArgs = OS.GetCmdlineUserArgs();

            var combinedArgs = new System.Collections.Generic.List<string>(args);
            combinedArgs.AddRange(userArgs);
            if (!combinedArgs.Contains("--run-tests") && !combinedArgs.Contains("-r"))
            {
                combinedArgs.Add("--run-tests");
            }

            var environment = TestEnvironment.From(combinedArgs.ToArray());
            var logWriter = new TestLogWriter();
            var log = new Log("GoTest", logWriter);

            await GoTest.RunTests(Assembly.GetExecutingAssembly(), Root, environment, log);

            // Yield frames before quitting to allow Godot's SceneTree deferred deletion queue
            // to process all QueueFree() requests, freeing nodes and their underlying CanvasItem/Physics RIDs.
            for (var i = 0; i < 2; i++)
            {
                await ToSignal(this, SignalName.ProcessFrame);
            }

            if (logWriter.HasErrors)
            {
                GD.PrintErr("Test suite failed.");
                Quit(1);
            }
            else
            {
                GD.Print("All tests passed successfully.");
                Quit(0);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Fatal error during test run: {ex}");
            Quit(1);
        }
    }
}
