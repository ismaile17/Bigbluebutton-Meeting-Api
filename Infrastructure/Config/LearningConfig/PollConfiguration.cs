using Domain.Entities.Learning;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasMany(p => p.PollVotes)
               .WithOne()
               .HasForeignKey(v => v.PollId);
    }
}
