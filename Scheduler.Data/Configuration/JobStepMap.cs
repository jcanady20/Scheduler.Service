using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scheduler.Data.Configuration
{
	class JobStepMap : EntityTypeConfiguration<Entities.JobStep>
	{
		public JobStepMap()
		{
			HasKey(x => x.Id);
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.Name).IsRequired();
			Property(x => x.Name).HasMaxLength(128);
			Property(x => x.SubSystem).IsRequired();
			Property(x => x.SubSystem).HasMaxLength(40);
			Property(X => X.DatabaseName).HasMaxLength(128);
		}
	}
}
