using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Data.Context
{
	public static class ContextFactory
	{
		public static IContext CreateContext()
		{
			return Context.Create();
		}
	}
}
