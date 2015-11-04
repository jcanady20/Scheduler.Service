using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Scheduler.Scheduling;
using System.Text.RegularExpressions;

namespace Scheduler.Service
{
    public partial class Program
	{
        protected const string TITLE = "Schedule Service Host";
        protected static Program m_this;
		protected static string m_prompt = ":>";
		protected static SchedulerService m_service;
		protected readonly static Type m_type = typeof(Program);
		protected static ICollection<MethodInfo> m_methods = null;

		internal Program()
		{
			m_this = this;
		}

		[STAThread]
		static void Main(string[] args)
		{
            
            if (Environment.UserInteractive == false)
			{
				ServiceBase[] ServicesToRun = new ServiceBase[] { new SchedulerService() };
				ServiceBase.Run(ServicesToRun);
			}
			else
			{
                AddConsoleTraceListener();
                CreateMethodCache();

				Console.Clear();
				Console.ForegroundColor = ConsoleColor.White;
				Console.BufferHeight = 300;
				Console.BufferWidth = 100;
                Console.Title = TITLE;

                if (args.Length > 0)
				{
					RunCommand(args);
					return;
				}
#if DEBUG
				Console.WriteLine("Starting Service from Main:");
				StartService();
				OpenConfig();
#endif
				MainLoop();
			}
		}
		static void MainLoop()
		{
			bool isCanceled = false;
            while (isCanceled == false)
            {
                Console.Write(m_prompt);
                var parms = ParseCommand(Console.ReadLine());
                if (parms.Length == 0)
                {
                    continue;
                }
                switch (parms[0].ToLower())
                {
                    case "exit":
                    case "quit":
                        isCanceled = true;
                        break;
                    default:
                        RunCommand(parms);
                        break;
                }
                Console.WriteLine("");
            }
		}
		static string[] ParseCommand(string args)
		{
            if(args == null)
            {
                args = String.Empty;
            }
			var parts = Regex.Matches(args, @"[\""].+?[\""]|[^ ]+")
				.Cast<Match>()
				.Select(x => x.Value.Replace("\"", ""))
				.ToArray();
			return parts;
		}
		static void RunCommand(string[] args)
		{
			string CallingMethod = args[0];
			var parms = args.Skip(1).Take(args.Count() - 1).ToArray();

			var method = m_methods.FirstOrDefault(x => String.Compare(x.Name, CallingMethod, true) == 0 && x.GetParameters().Length == parms.Length);
			if(method == null)
			{
				Console.WriteLine("Unknown Command");
				Program.Help();
				return;
			}

			try
			{
				method.Invoke(m_this, parms);
				return;
			}
			catch (Exception e)
			{
				if (e.InnerException != null)
				{
					Console.WriteLine();
					WriteExceptions(e.InnerException);
				}
				return;
			}
		}

		[Description("Shows Help for All Commands")]
		static void Help()
		{
			Console.WriteLine("Valid Commands");
			foreach (MethodInfo m in m_methods)
			{
				DescriptionAttribute[] attribs = (DescriptionAttribute[])m.GetCustomAttributes(typeof(DescriptionAttribute), false);
				if (attribs != null && attribs.Length > 0)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write(m.Name);
					ParameterInfo[] parm = m.GetParameters();
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write("(");
					for (int i = 0; i < parm.Length; i++)
					{
						if (i > 0)
							Console.Write(", ");

						Console.Write("({0}){1}", parm[i].ParameterType.Name, parm[i].Name);
					}
					Console.Write(")");
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("\n\t{0}", attribs[0].Description);
				}
			}
		}

		[Description("Clears the Current display Buffer")]
		static void Clear()
		{
			Console.Clear();
		}

		[Description("Quits out of the application")]
		static void Quit()
		{
			return;
		}

		[Description("Open Application Log Folder")]
		static void OpenLogFolder()
		{
            var logPath = Path.Combine(GetCurrentPath(), "ApplicationLogs");
            if (Directory.Exists(logPath) == false)
            {
                Directory.CreateDirectory(logPath);
            }
            Process.Start(logPath);
        }

		[Description("Open Scheduler Configuration")]
		static void OpenConfig()
		{
			var baseAddress = Scheduler.HttpService.ServiceUrl.GetServiceUrl();
			baseAddress += "/configuration";
			Process.Start(baseAddress);
		}

		static TimeSpan CalculateEta(DateTime startTime, int totalItems, int completeItems)
		{
            //	Avoid Divide by Zero Errors
            completeItems = (completeItems == 0) ? 1 : completeItems;
			var _itemduration = (int)(DateTime.Now.Subtract(startTime).TotalMilliseconds / completeItems);
            var _estimatedMilliseconds = (double)((totalItems - completeItems) * _itemduration);
            return TimeSpan.FromMilliseconds(_estimatedMilliseconds);
		}

		static void WriteExceptions(Exception e)
		{
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Red;

			Trace.Write("Source:");
			Trace.Write(e.Source);
			Trace.WriteLine("\nMessage:");
			Trace.Write(e.Message);
			Trace.WriteLine("\nStack Trace:");
			Trace.Write(e.StackTrace);
			Trace.WriteLine("\nUser Defined Data:");
			foreach (System.Collections.DictionaryEntry de in e.Data)
			{
				Trace.WriteLine(string.Format("[{0}] :: {1}", de.Key, de.Value));
			}
			if (e.InnerException != null)
			{
				WriteExceptions(e.InnerException);
			}
			Console.ForegroundColor = ConsoleColor.White;
		}

		public static string GetCurrentPath()
		{
			var asm = Assembly.GetExecutingAssembly();
			var fi = new FileInfo(asm.Location);
			return fi.DirectoryName;
		}

		static void CreateMethodCache()
		{
			m_methods = m_type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToList();
		}

		static void AddConsoleTraceListener()
		{
			var writer = new TextWriterTraceListener(Console.Out);
			Trace.Listeners.Add(writer);
		}
	}
}
