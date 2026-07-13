using System.Diagnostics;
using System.ComponentModel;

namespace Scheduler.Tasks;

[DisplayName("Operating System Task")]
[Description("Run an executable program")]
public class OSTask : JobTaskBase
{
  public OSTask()
  { }

  public override void OnExecute()
  {
    if (String.IsNullOrEmpty(_taskStep.Command))
    {
      throw new Exception("Aborting Command Execution, the Command was Empty");
    }
    var process = new Process();
    process.StartInfo = new ProcessStartInfo
    {
      CreateNoWindow = true,
      UseShellExecute = true,
      WindowStyle = ProcessWindowStyle.Hidden,
      FileName = _taskStep.Command
    };
    process.Start();
    //process.WaitForExit();
  }
}
