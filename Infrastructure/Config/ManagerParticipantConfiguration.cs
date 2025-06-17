using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class ManagerParticipantConfiguration : IEntityTypeConfiguration<ManagerParticipant>
    {
        public void Configure(EntityTypeBuilder<ManagerParticipant> builder)
        {
            // Tablo adı ve anahtar belirleme
            builder.ToTable("ManagerParticipants");
            builder.HasKey(mp => new { mp.ManagerId, mp.ParticipantId });

            // Manager ilişkisi
            builder.HasOne(mp => mp.Manager)
                   .WithMany()
                   .HasForeignKey(mp => mp.ManagerId);

            // Participant ilişkisi
            builder.HasOne(mp => mp.Participant)
                   .WithMany()
                   .HasForeignKey(mp => mp.ParticipantId);


        }
    }
}