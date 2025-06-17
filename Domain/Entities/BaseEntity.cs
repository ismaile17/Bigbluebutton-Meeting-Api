namespace Domain.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public int? CreatedBy { get; set; }

        public short IsActive { get; set; } = 1;

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedTime { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
