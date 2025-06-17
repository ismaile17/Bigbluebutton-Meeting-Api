using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class TicketStatusHistoryConfiguration : IEntityTypeConfiguration<TicketStatusHistory>
    {
        public void Configure(EntityTypeBuilder<TicketStatusHistory> builder)
        {
            builder.HasKey(m => m.Id); // Birincil anahtar

            builder.Property(h => h.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(h => h.ChangedDate)
                .IsRequired();

            builder.HasOne(h => h.Ticket)
                .WithMany(t => t.TicketStatusHistories)
                .HasForeignKey(h => h.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
