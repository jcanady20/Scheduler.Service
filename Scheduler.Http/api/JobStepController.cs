using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

using Scheduler.Data;
using Scheduler.Data.Entities;

namespace Scheduler.Http.api
{
	[RoutePrefix("api/steps")]
	public class JobStepController : BaseApiController
	{
		[Route("{id}")]
		[HttpGet]
		public async Task<IHttpActionResult> Get(int id)
		{
			try
			{
				var item = await m_db.JobSteps.FirstOrDefaultAsync(x => x.Id == id);
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
		public async Task<IHttpActionResult> Post([FromBody] Data.Entities.JobStep taskStep)
		{
			try
			{
				var validation = m_db.GetValidationResult(taskStep);
				if (validation.IsValid)
				{
					m_db.JobSteps.Add(taskStep);
					await m_db.SaveChangesAsync();
					return Ok(taskStep);
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
		public async Task<IHttpActionResult> Put(Guid id, [FromBody] Data.Entities.JobStep taskStep)
		{
			try
			{
				var validation = m_db.GetValidationResult(taskStep);
				if (validation.IsValid)
				{
					m_db.JobSteps.Attach(taskStep);
					m_db.SetModified(taskStep);
					await m_db.SaveChangesAsync();

					return Ok(taskStep);
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
				var item = await m_db.JobSteps.FirstOrDefaultAsync(x => x.Id == id);
				if (item == null)
				{
					return NotFound();
				}

				m_db.JobSteps.Remove(item);
				await m_db.SaveChangesAsync();
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
