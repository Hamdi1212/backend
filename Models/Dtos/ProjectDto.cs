
    namespace Checklist.Models.Dtos
    {
        public class ProjectDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;

            // Optional: include lines if we need them when displaying project details
            public List<LineDto>? Lines { get; set; }
        }

        public class ProjectCreateDto
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        public class ProjectUpdateDto
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }


