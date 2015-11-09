using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;

using Scheduler.Data;
using Scheduler.Data.Context;
using Scheduler.Data.Entities;
using Scheduler.Data.Models;
using Scheduler.Extensions;
using Scheduler.Logging;

namespace Scheduler.Tasks
{
    public class TaskManager
    {
        private ILogger m_logger;
        private static object m_lockObj = new object();
        private IDictionary<string, Type> m_taskPlugins;
        private ICollection<PluginDetail> m_pluginDetails;

        public TaskManager(ILogger logger)
        {
            m_taskPlugins = new Dictionary<string, Type>();
            m_pluginDetails = new List<PluginDetail>();
            m_logger = logger;
            LoadTasks();
        }

        public IEnumerable<PluginDetail> TaskPlugins
        {
            get
            {
                return m_pluginDetails;
            }
        }

        protected virtual void LoadTasks()
        {
            m_logger.Info("Started :: Loading Task Plugins");

            try
            {
                LoadFromCurrentAssembly();
                LoadFromPlugins();
            }
            catch (Exception e)
            {
                m_logger.Error(e);
            }

            m_logger.Info("Completed :: Loading Task Plugins");
        }

        private string GetPluginPath()
        {
            var result = string.Empty;
            result = ConfigurationManager.AppSettings["PluginPath"];
            if (String.IsNullOrEmpty(result) == false && Directory.Exists(result))
            {
                return result;
            }
            Uri codeBase = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            result = Path.GetDirectoryName(codeBase.LocalPath);
            result = Path.Combine(result, "plugins");
            return result;
        }

        private void LoadFromCurrentAssembly()
        {
            var assembly = typeof(TaskManager).Assembly;
            LoadTasksFromAssembly(assembly);
        }

        private void LoadFromPlugins()
        {
            var path = GetPluginPath();
            if (Directory.Exists(path) == false)
            {
                m_logger.Warn("Specified Plugin Path does not exists: {0}", path);
                return;
            }
            var dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles("*.dll");
            foreach (var fi in files)
            {
                LoadPluginFromFileInfo(fi);
            }
        }

        private void LoadPluginFromFileInfo(FileInfo fileinfo)
        {
            try
            {
                var assembly = Assembly.LoadFile(fileinfo.FullName);
                LoadTasksFromAssembly(assembly);
            }
            catch (Exception e)
            {
                m_logger.Error(e);
            }
        }

        private void LoadTasksFromAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var types = assembly
                .GetTypes()
                .Where(x => typeof(IJobTask).IsAssignableFrom(x))
                .Where(x => x.IsAbstract == false)
                .Where(x => x.IsInterface == false);

            types.ForEach(t => {
                    var displayName = t.GetCustomAttribute<DisplayNameAttribute>();
                    var description = t.GetCustomAttribute<DescriptionAttribute>();
                    var detail = new PluginDetail();

                    detail.DisplayName = displayName?.DisplayName ?? t.Name;
                    detail.Description = description?.Description ?? t.Name;
                    detail.SubSystem = t.Name;
                    detail.Type = t;

                    m_pluginDetails.Add(detail);
                    m_taskPlugins.Add(t.Name, t);
                });
        }

        public IJobTask CreateTask(IContext db, JobStep taskStep)
        {
            IJobTask task = null;
            if (taskStep == null)
            {
                return null;
            }

            if (m_taskPlugins.ContainsKey(taskStep.SubSystem) == false)
            {
                m_logger.Warn("Unable to locate specified Subsystem [{0}]", taskStep.SubSystem);
                return null;
            }
            var t = m_taskPlugins[taskStep.SubSystem];
            task = (IJobTask)Activator.CreateInstance(t);
            return task.Create(db, taskStep);
        }
    }
}
