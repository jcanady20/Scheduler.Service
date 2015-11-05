﻿using Scheduler.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PShell = System.Management.Automation;
using System.Collections.ObjectModel;

namespace Scheduler.Tasks.PowerShell
{
    public class PSTask : BaseTask
    {
        public override void OnExecute()
        {
            if (String.IsNullOrEmpty(m_taskStep.Command))
            {
                throw new Exception("Aborting Command Execution, the Command was Empty");
            }

            using (var psInstance = PShell.PowerShell.Create())
            {

                psInstance.AddScript(m_taskStep.Command);
                var output = psInstance.Invoke();
                if (psInstance.Streams.Error.Count > 0)
                {
                    foreach(var er in psInstance.Streams.Error)
                    {
                        Log.Error(er.Exception);
                    }
                }
            }
        }
    }
}
