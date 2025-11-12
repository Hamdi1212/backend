namespace Checklist.Models.Dtos
{
    public class LineDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public string? ProjectName { get; set; }
    }

    public class LineCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
    }

    public class LineUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
    }
}
