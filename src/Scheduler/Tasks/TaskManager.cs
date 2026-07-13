using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Scheduler.Data.Context;
using Scheduler.Data.Entities;
using Scheduler.Data.Models;
using Scheduler.Extensions;

namespace Scheduler.Tasks;

public class TaskManager
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private IDictionary<string, Type> _taskPlugins;
    private ICollection<PluginDetail> _pluginDetails;

    public TaskManager(ILogger logger, IConfiguration configuration)
    {
      _taskPlugins = new Dictionary<string, Type>();
      _pluginDetails = new List<PluginDetail>();
      _logger = logger;
      _configuration = configuration;
      LoadTasks();
    }

    public IEnumerable<PluginDetail> TaskPlugins
    {
      get
      {
        return _pluginDetails;
      }
    }

    protected virtual void LoadTasks()
    {
      _logger.LogInformation("Started :: Loading Task Plugins");

      try
      {
        LoadFromCurrentAssembly();
        LoadFromPlugins();
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message, e);
      }

      _logger.LogInformation("Completed :: Loading Task Plugins");
    }

    private string GetPluginPath()
    {
        var result = string.Empty;
        result = _configuration["PluginPath"];
        if (String.IsNullOrEmpty(result) == false && Directory.Exists(result))
        {
            return result;
        }
        Uri codeBase = new Uri(Assembly.GetExecutingAssembly().Location);
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
        _logger.LogWarning("Specified Plugin Path does not exists: {Path}", path);
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
        _logger.LogError(e.Message, e);
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
        var detail = new PluginDetail
        {
          DisplayName = displayName?.DisplayName ?? t.Name,
          Description = description?.Description ?? t.Name,
          SubSystem = t.Name,
          Type = t
        };

        _pluginDetails.Add(detail);
          _taskPlugins.Add(t.Name, t);
        });
    }

    public IJobTask CreateTask(ScheduleContext db, JobStep taskStep)
    {
      IJobTask task = null;
      if (taskStep == null)
      {
        return null;
      }

      if (_taskPlugins.ContainsKey(taskStep.SubSystem) == false)
      {
        _logger.LogWarning("Unable to locate specified Subsystem [{SubSystem}]", taskStep.SubSystem);
        return null;
      }
      var t = _taskPlugins[taskStep.SubSystem];
      task = (IJobTask)Activator.CreateInstance(t);
      return task.Create(db, taskStep, _logger);
    }
}
