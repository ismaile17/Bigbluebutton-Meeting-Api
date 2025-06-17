using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class MoneyTransferFormConfiguration : IEntityTypeConfiguration<MoneyTransferForm>
    {
        public void Configure(EntityTypeBuilder<MoneyTransferForm> builder)
        {
            builder.HasKey(a => a.Id);

        }
    }
}
