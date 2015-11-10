using System;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Scheduler.Data.Queries;
using Scheduler.Data;
using Scheduler.Data.Entities;
using Scheduler.Data.Models;

namespace Scheduler.Http.Api
{
    [RoutePrefix("api/jobschedules")]
    public class JobScheduleController : BaseApiController
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
        public async Task<IHttpActionResult> Post([FromBody] JobSchedule jobSchedule)
        {
            try
            {
                var validation = m_db.GetValidationResult(jobSchedule);
                if (validation.IsValid)
                {
                    m_db.JobSchedules.Add(jobSchedule);
                    await m_db.SaveChangesAsync();
                    return Ok(jobSchedule);
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
        public async Task<IHttpActionResult> Put([FromBody] JobSchedule jobSchedule)
        {
            try
            {
                var validation = m_db.GetValidationResult(jobSchedule);
                if (validation.IsValid)
                {
                    m_db.JobSchedules.Attach(jobSchedule);
                    m_db.SetModified(jobSchedule);
                    await m_db.SaveChangesAsync();

                    return Ok(jobSchedule);
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

        [Route("{jobId}/{scheduleId}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(int jobId, int scheduleId)
        {
            try
            {
                var item = await m_db.JobSchedules.FirstOrDefaultAsync(x => x.JobId == jobId && x.ScheduleId == scheduleId);
                if (item == null)
                {
                    return NotFound();
                }

                m_db.JobSchedules.Remove(item);
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
