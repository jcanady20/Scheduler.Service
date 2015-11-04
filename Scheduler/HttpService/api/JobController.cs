using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

using Scheduler.Data;
using Scheduler.Data.Entities;
using Scheduler.Data.Extensions;
using Scheduler.Data.Queries;


namespace Scheduler.HttpService.api
{
	[RoutePrefix("api/jobs")]
	public class JobController : BaseApiController
	{
		[Route("")]
		[HttpGet]
		public IHttpActionResult Get()
		{
			try
			{
				var items = m_db.Jobs;
				return Ok(items);
			}
			catch(Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("{id}")]
		[HttpGet]
		public IHttpActionResult Get(Guid id)
		{
			try
			{
				var item = m_db.Jobs.FirstOrDefault(x => x.Id == id);
				return Ok(item);
			}
			catch (Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("history")]
		[HttpGet]
		public IHttpActionResult GetJobHistory([FromUri] Data.Models.JobHistorySearch model)
		{
			try
			{
				var items = m_db.JobHistorySearch(model);
				return Ok(items);
			}
			catch (Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("activity")]
		[HttpGet]
		public IHttpActionResult GetJobActivity()
		{
			try
			{
				var items = m_db.JobActivity.ToList();
				return Ok(items);
			}
			catch(Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("subsystems")]
		[HttpGet]
		public IHttpActionResult GetSubSystems()
		{
			try
			{
				var items = Scheduling.JobTaskFactory.Instance.TaskPlugins.ToList();
				return Ok(items);
			}
			catch(Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("steps/{id}")]
		[HttpGet]
		public IHttpActionResult GetJobSteps(Guid id)
		{
			try
			{
				var items = m_db.JobSteps.Where(x => x.JobId == id);
				return Ok(items);
			}
			catch (Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("schedules/{id}")]
		[HttpGet]
		public IHttpActionResult GetJobSchedules(Guid id)
		{
			try
			{
				var items = m_db.GetJobSchedules(id);
				return Ok(items);
			}
			catch(Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("start/{id}")]
		[HttpGet]
		public IHttpActionResult ExecuteJob(Guid id)
		{
			try
			{
				var job = m_db.Jobs.AsNoTracking().Include(r => r.JobSteps).AsNoTracking().FirstOrDefault(x => x.Id == id);
				Scheduling.JobEngine.Instance.Add(job, null);
				return Ok(new { success = true });
			}
			catch(Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("")]
		[HttpPost]
		public IHttpActionResult Post([FromBody] Data.Entities.Job job)
		{
			try
			{
				var validation = m_db.GetValidationResult(job);
				if (validation.IsValid)
				{
					m_db.Jobs.Add(job);
					m_db.SaveChanges();
					return Ok(job);
				}
				else
				{
					AddErrorsToModelState(validation, ModelState);
					return BadRequest(ModelState);
				}
			}
			catch (Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("report")]
		[HttpPost]
		public IHttpActionResult ReportResponse([FromBody]Data.Models.WebTaskResponse response)
		{
			try
			{
				Scheduler.Scheduling.WebTaskRespponseManager.Instance.ReportResponse(response);
				return Ok();
			}
			catch(Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("{id}")]
		[HttpPut, HttpPatch]
		public IHttpActionResult Put(Guid id, [FromBody] Data.Entities.Job job)
		{
			try
			{
				var validation = m_db.GetValidationResult(job);
				if (validation.IsValid)
				{
					m_db.Jobs.Attach(job);
					m_db.SetModified(job);
					m_db.SaveChanges();

					return Ok(job);
				}
				else
				{
					AddErrorsToModelState(validation, ModelState);
					return BadRequest(ModelState);
				}
			}
			catch (Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("{id}")]
		[HttpDelete]
		public IHttpActionResult Delete(Guid id)
		{
			try
			{
				var item = m_db.Jobs.FirstOrDefault(x => x.Id == id);
				if (item == null)
				{
					return NotFound();
				}

				m_db.Jobs.Remove(item);
				m_db.SaveChanges();
				return Ok(item);
			}
			catch (Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}
	}
}
