using System;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Scheduler.Data.Queries;


namespace Scheduler.Http.Api
{
    [RoutePrefix("api/jobs")]
	public class JobController : BaseApiController
	{
		[Route("")]
		[HttpGet]
		public async Task<IHttpActionResult> Get()
		{
			try
			{
				var items = await m_db.Jobs.ToListAsync();
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
		public async Task<IHttpActionResult> Get(int id)
		{
			try
			{
				var item = await m_db.Jobs.FirstOrDefaultAsync(x => x.Id == id);
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
		public async Task<IHttpActionResult> GetJobHistory([FromUri] Data.Models.JobHistorySearch model)
		{
			try
			{
				var items = await Task.FromResult(m_db.JobHistorySearch(model));
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
		public async Task<IHttpActionResult> GetJobActivity()
		{
			try
			{
				var items = await m_db.JobActivity.ToListAsync();
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
		public async Task<IHttpActionResult> GetSubSystems()
		{
			try
			{
                var items = await Task.FromResult(Scheduler.Jobs.JobEngine.Instance.TaskManager.TaskPlugins.ToList());
				return Ok(items);
			}
			catch(Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("{id}/steps")]
		[HttpGet]
		public async Task<IHttpActionResult> GetJobSteps(int id)
		{
			try
			{
				var items = await m_db.JobSteps.Where(x => x.JobId == id).ToListAsync();
				return Ok(items);
			}
			catch (Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("{id}/schedules")]
		[HttpGet]
		public async Task<IHttpActionResult> GetJobSchedules(int id)
		{
			try
			{
                var items = await m_db.GetJobSchedules(id).ToListAsync();
				return Ok(items);
			}
			catch(Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

		[Route("{id}/start")]
		[HttpGet]
		public async Task<IHttpActionResult> ExecuteJob(int id)
		{
			try
			{
                m_db.Configuration.ProxyCreationEnabled = false;
                m_db.Configuration.AutoDetectChangesEnabled = false;
				var job = await m_db.Jobs.Include(r => r.JobSteps).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
				Jobs.JobEngine.Instance.Add(job, null);
				return Ok(new { success = true });
			}
			catch(Exception e)
			{
				m_logger.Error(e);
				return new ExceptionResult(e, this);
			}
		}

        [Route("{id}/cacnel")]
        [HttpPost]
        public IHttpActionResult CacnelJob(int id)
        {
            try
            {
                var jobExecution = Jobs.JobEngine.Instance.CurrentActivity.FirstOrDefault(x => x.JobId == id);
                Jobs.JobEngine.Instance.CancelExecution(jobExecution);
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
		public async Task<IHttpActionResult> Post([FromBody] Data.Entities.Job job)
		{
			try
			{
				var validation = m_db.GetValidationResult(job);
				if (validation.IsValid)
				{
					m_db.Jobs.Add(job);
					await m_db.SaveChangesAsync();
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
		[HttpPut, HttpPatch]
		public async Task<IHttpActionResult> Put(Guid id, [FromBody] Data.Entities.Job job)
		{
			try
			{
				var validation = m_db.GetValidationResult(job);
				if (validation.IsValid)
				{
					m_db.Jobs.Attach(job);
					m_db.SetModified(job);
					await m_db.SaveChangesAsync();

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

		[Route("activity/{id}")]
		[HttpDelete]
		public async Task<IHttpActionResult> Delete(int id)
		{
			try
			{
				var item = await m_db.Jobs.FirstOrDefaultAsync(x => x.Id == id);
				if (item == null)
				{
					return NotFound();
				}

				m_db.Jobs.Remove(item);
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
