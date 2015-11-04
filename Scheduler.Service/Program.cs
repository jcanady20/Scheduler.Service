using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.ServiceProcess;
using Scheduler.Scheduling;
using System.Text.RegularExpressions;

namespace Scheduler.Service
{
	public partial class Program
	{
		protected static Program m_this;
		protected static string m_prompt = ":>";
		protected static SchedulerService m_service;
		protected static Type m_type = typeof(Program);
		protected static ICollection<MethodInfo> m_methods = null;

		internal Program()
		{
			m_this = this;
		}

		[STAThread]
		static void Main(string[] args)
		{
			AddTraceListeners();

			if (Environment.UserInteractive == false)
			{
				ServiceBase[] ServicesToRun = new ServiceBase[] { new SchedulerService() };
				ServiceBase.Run(ServicesToRun);
			}
			else
			{

				CreateMethodCache();

				Console.Clear();
				Console.ForegroundColor = ConsoleColor.White;
				Console.BufferHeight = 300;
				Console.BufferWidth = 100;
				Console.Title = "Visualutions ServiceProvider";

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
			bool pContinue = true;
			while (pContinue)
			{
				Console.Write(m_prompt);

				var parms = ParseCommand(Console.ReadLine());
				if (parms.Length == 0)
					continue;

				switch (parms[0].ToLower())
				{
					case "exit":
					case "quit":
						pContinue = false;
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

		[Description("Install Service")]
		static void InstallService()
		{
			try
			{
				// "/LogFile=" - to suppress install log creation
				System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/LogFile=", System.Reflection.Assembly.GetExecutingAssembly().Location });
			}
			catch { }
		}

		[Description("Uninstall Service")]
		static void UninstallService()
		{
			try
			{
				// "/LogFile=" - to suppress uninstall log creation
				System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", "/LogFile=", System.Reflection.Assembly.GetExecutingAssembly().Location });
			}
			catch { }
		}

		[Description("Start Service Interactively")]
		static void StartService()
		{
			m_service = new SchedulerService();
			m_service.Start();
		}

		[Description("Stops the Service")]
		static void StopService()
		{
			if (m_service != null && m_service.CanStop)
				m_service.Stop();
		}

		[Description("Create the Eventlog used by the service")]
		static void CreateEventLog()
		{
			var eventLogSource = SchedulerService.EVENTLOGSOURCE;
			var eventLogName = SchedulerService.EVENTLOGNAME;
			if(!EventLog.SourceExists(eventLogSource))
			{
				EventLog.CreateEventSource(eventLogSource, eventLogName);
			}
		}

		[Description("List Local Drives")]
		static void LocalDrives()
		{
			string[] drives = Environment.GetLogicalDrives();
			IEnumerable<string> strs = drives.Select(s => s.Replace(":\\", ""));
			foreach (String s in strs)
			{
				System.IO.DriveInfo drvi = new System.IO.DriveInfo(s);
				if (drvi.DriveType == DriveType.CDRom)
					continue;
				Console.WriteLine("{0}:\\", s);
			}
		}

		[Description("Open Application Log Folder")]
		static void OpenLogFolder()
		{
			Process.Start(Path.Combine(GetCurrentPath(), "ApplicationLogs"));
		}

		[Description("Open Scheduler Configuration")]
		static void OpenConfig()
		{
			var baseAddress = Scheduler.HttpService.ServiceUrl.GetServiceUrl();
			baseAddress += "/configuration";
			Process.Start(baseAddress);
		}

		static void LogMessage(string msg)
		{
			Trace.WriteLine(msg);
		}

		static TimeSpan CalculateEta(DateTime startTime, int totalItems, int completeItems)
		{
			TimeSpan _eta = TimeSpan.MinValue;
			//	Avoid Divide by Zero Errors
			if (completeItems > 0)
			{
				int _itemduration = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds / completeItems;
				_eta = TimeSpan.FromMilliseconds((double)((totalItems - completeItems) * _itemduration));
			}
			return _eta;
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

		static void HexDump(byte[] bytes)
		{
			for (int line = 0; line < bytes.Length; line += 16)
			{
				byte[] lineBytes = bytes.Skip(line).Take(16).ToArray();
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendFormat("{0:x8} ", line);
				sb.Append(string.Join(" ", lineBytes.Select(b => b.ToString("x2")).ToArray()).PadRight(16 * 3));
				sb.Append(" ");
				sb.Append(new string(lineBytes.Select(b => b < 32 ? '.' : (char)b).ToArray()));
				Console.WriteLine(sb);
			}
		}

		static void writeHeader<T>(T data)
		{
			if (data == null)
			{
				Console.WriteLine("Unable to create Header from Null Object");
				return;
			}
			var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (var prop in props)
			{
				Console.Write("{0, 8} ", prop.Name);
			}
			Console.WriteLine();
		}

		static void writeCollectionData<T>(IEnumerable<T> data)
		{
			if (data == null)
			{
				Console.WriteLine("Unable to write data from Null Object");
				return;
			}
			var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var i = 0;
			foreach (var t in data)
			{
				++i;
				Console.Write("{0}: ", i);
				writeData(t, props);
				Console.WriteLine();
			}
		}

		static void writeData<T>(T data, IEnumerable<PropertyInfo> propertyInfo = null)
		{
			if (data == null)
			{
				Console.WriteLine("Unable to write data from Null Object");
				return;
			}
			var props = (propertyInfo != null) ? propertyInfo : typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (var prop in props)
			{
				Console.Write("{0, 8} ", prop.GetValue(data));
			}
		}

		static void CreateMethodCache()
		{
			m_methods = m_type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToList();
		}

		static void AddTraceListeners()
		{
			if (Environment.UserInteractive)
			{
				TextWriterTraceListener CWriter = new TextWriterTraceListener(Console.Out);
				Trace.Listeners.Add(CWriter);
			}
		}
	}
}
