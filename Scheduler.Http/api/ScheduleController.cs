using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Data.Entity;

using Scheduler.Data;
using Scheduler.Data.Entities;

namespace Scheduler.Http.Api
{
	[RoutePrefix("api/schedules")]
	public class ScheduleController : BaseApiController
	{
		[Route("{id}")]
		[HttpGet]
		public async Task<IHttpActionResult> Get(int id)
		{
			try
			{
				var item = await m_db.Schedules.FirstOrDefaultAsync(x => x.Id == id);
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
		public async Task<IHttpActionResult> Post([FromBody] Schedule schedule)
		{
			try
			{
				var validation = m_db.GetValidationResult(schedule);
				if (validation.IsValid)
				{
					m_db.Schedules.Add(schedule);
					await m_db.SaveChangesAsync();
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
		public async Task<IHttpActionResult> Put(int id, [FromBody] Schedule schedule)
		{
			try
			{
				var validation = m_db.GetValidationResult(schedule);
				if (validation.IsValid)
				{
					m_db.Schedules.Attach(schedule);
					m_db.SetModified(schedule);
					await m_db.SaveChangesAsync();

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
		public async Task<IHttpActionResult> Delete(int id)
		{
			try
			{
				var item = await m_db.Schedules.FirstOrDefaultAsync(x => x.Id == id);
				if (item == null)
				{
					return NotFound();
				}

				m_db.Schedules.Remove(item);
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
