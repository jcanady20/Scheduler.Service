using PShell = System.Management.Automation;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Scheduler.Tasks.PowerShell;

[DisplayName("PowerShell Task")]
[Description("Executes a PowerShell Script. Execution only.")]
public class PSTask : JobTaskBase
{
    public override void OnExecute()
    {
        if (String.IsNullOrEmpty(_taskStep.Command))
        {
            throw new Exception("Aborting Command Execution, the Command was Empty");
        }

        using (var psInstance = PShell.PowerShell.Create())
        {
            psInstance.AddScript(_taskStep.Command);
            var output = psInstance.Invoke();
            if (psInstance.Streams.Error.Count > 0)
            {
                foreach(var er in psInstance.Streams.Error)
                {
                    Logger.LogError(er.Exception, er.Exception.Message);
                }
            }
        }
    }
}
