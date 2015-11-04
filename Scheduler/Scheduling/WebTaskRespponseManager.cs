using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Scheduler.Data.Context;
using Scheduler.Data.Queries;
using Scheduler.Logging;

namespace Scheduler.Scheduling
{
    public class WebTaskRespponseManager
	{
		private ILogger m_logger;
		private static readonly int m_workDelay = 1000 * 60;
		private static readonly object m_lockObj = new object();
		private static WebTaskRespponseManager m_instance;
		private ICollection<PendingResponse> m_pendingResponses;
		private Thread m_worker;
		private WebTaskRespponseManager()
		{
			m_logger = new NLogger();
			m_pendingResponses = new List<PendingResponse>();
			m_worker = new Thread(new ThreadStart(DoWork));
			m_worker.Start();
		}
		public static WebTaskRespponseManager Instance
		{
			get
			{
				lock(m_lockObj)
				{
					if (m_instance == null)
					{
						m_instance = new WebTaskRespponseManager();
					}
					return m_instance;
				}				
			}
		}

		//	Clean up Stale requests
		private void DoWork()
		{
			while(true)
			{
				RemoveStalegResponses();
				Thread.Sleep(m_workDelay);
			}
		}

		public void AwaitResponse(Guid jobId ,Guid requestId, int stepId, string name)
		{
			var pending = new PendingResponse(jobId, requestId, stepId, name);
			this.m_pendingResponses.Add(pending);
		}
		
		public void ReportResponse(Data.Models.WebTaskResponse response)
		{
			var pending = m_pendingResponses.FirstOrDefault(x => x.RequestId == response.RequestId);
			if(pending == null)
			{
				return;
			}
			m_pendingResponses.Remove(pending);
			using(var db = ContextFactory.CreateContext())
			{
				db.AddJobHistory(pending.JobId, pending.StepId, pending.Name, response.OutCome, response.Message, pending.RunDateTime, response.Duration);
			}
		}

		private void RemoveStalegResponses()
		{
			var minutes = 5;
			try
			{
				var responses = m_pendingResponses.Where(x => DateTime.Now.Subtract(x.Created).TotalMinutes >= minutes).Take(10).ToList();
				foreach (var resp in responses)
				{
					m_pendingResponses.Remove(resp);
				}
			}
			catch(Exception e)
			{
				m_logger.Error(e);
			}
		}

		internal class PendingResponse
		{
			internal PendingResponse()
			{
				this.RunDateTime = DateTime.Now;
				this.Created = DateTime.Now;
			}
			internal PendingResponse(Guid jobId, Guid requestId, int stepId, string name) : this()
			{
				JobId = jobId;
				RequestId = requestId;
				StepId = stepId;
				Name = name;
			}
			internal Guid JobId { get; set; }
			internal Guid RequestId { get; set; }
			internal int StepId { get; set; }
			internal string Name { get; set; }
			internal DateTime RunDateTime { get; private set; }
			internal DateTime Created { get; private set; }
		}

	}
}
