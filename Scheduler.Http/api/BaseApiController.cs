using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Scheduler.Data.Context;
using System.Data.Entity.Validation;
using System.Web.Http.ModelBinding;
using Scheduler.Logging;

namespace Scheduler.Http.api
{
    public class BaseApiController : ApiController
	{
		internal ILogger m_logger;
		internal IContext m_db;
		internal BaseApiController()
		{
            m_logger = LogProvider.Instance.Logger;
			m_db = ContextFactory.CreateContext();
		}

		internal void AddErrorsToModelState(DbEntityValidationResult validation, ModelStateDictionary state)
		{
			if (validation == null || state == null || validation.IsValid)
			{
				return;
			}
			var errors = validation.ValidationErrors.Select(r => new { Property = r.PropertyName, ErrorMessage = r.ErrorMessage });
			foreach (var error in errors)
			{
				ModelState.AddModelError(error.Property, error.ErrorMessage);
			}
		}
		internal void AddErrorsToModelState(IEnumerable<DbEntityValidationResult> errors, ModelStateDictionary state)
		{
			foreach (var val in errors)
			{
				AddErrorsToModelState(val, state);
			}
		}
		internal void AddErrorToModelState(ModelStateDictionary state, string property, string message)
		{
			if (state == null)
			{
				return;
			}
			state.AddModelError(property, message);
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(m_db != null)
				{
					m_db.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
