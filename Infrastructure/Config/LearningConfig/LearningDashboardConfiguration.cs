using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
    public class LearningDashboardConfiguration : IEntityTypeConfiguration<LearningDashboard>
    {
        public void Configure(EntityTypeBuilder<LearningDashboard> builder)
        {
            builder.HasKey(ld => ld.Id);
        }
    }
}
