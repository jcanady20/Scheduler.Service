using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

using Scheduler.Data;
using Scheduler.Data.Entities;

namespace Scheduler.HttpService.api
{
	[RoutePrefix("api/jobschedules")]
	public class JobScheduleController : BaseApiController
	{
		[Route("{id}")]
		[HttpGet]
		public IHttpActionResult Get(Guid id)
		{
			try
			{
				var item = m_db.JobSchedules.FirstOrDefault(x => x.Id == id);
				return Ok(item);
			}
			catch (Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("")]
		[HttpPost]
		public IHttpActionResult Post([FromBody] JobSchedule schedule)
		{
			try
			{
				var validation = m_db.GetValidationResult(schedule);
				if (validation.IsValid)
				{
					m_db.JobSchedules.Add(schedule);
					m_db.SaveChanges();
					return Ok(schedule);
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
		[HttpPut, HttpPatch]
		public IHttpActionResult Put(Guid id, [FromBody] JobSchedule schedule)
		{
			try
			{
				var validation = m_db.GetValidationResult(schedule);
				if (validation.IsValid)
				{
					m_db.JobSchedules.Attach(schedule);
					m_db.SetModified(schedule);
					m_db.SaveChanges();

					return Ok(schedule);
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
				var item = m_db.JobSchedules.FirstOrDefault(x => x.Id == id);
				if (item == null)
				{
					return NotFound();
				}

				m_db.JobSchedules.Remove(item);
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
