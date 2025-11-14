using System.ComponentModel.DataAnnotations;

namespace Checklist.Models
{
    public class Question
    {
        [Key]
        public Guid Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public Guid TemplateId { get; set; }
        
        public Template Template { get; set; }
        public ICollection<Answer>? Answers { get; set; }
    }
}
