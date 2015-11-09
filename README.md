# Scheduler.Service
Scheduler service is windows Scheduling service similar to Sql Server Agent.
The Service has several built-in tasks that can be scheduled to execute 
based on the given schedule interval.
Current includes tasks include a Sql Script runner, a process runner, and a PowerShell script processor.

Schedules can be configured based on a number of configuration options.
On Startup,
Daily (at specified time, or recurring minutes, hours)
Weekly (at specified time, or recurring miunutes, hours, days)
Monthly (interval, or recurring)

Configuration is handled through embedded MVC Razor Views and Resetful services provided by WebApi. These services are hosted using Owin Services and custom middleware.

