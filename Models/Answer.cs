using System.ComponentModel.DataAnnotations;

namespace Checklist.Models
{
    public class Answer
    {
        [Key]
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public Guid ChecklistId { get; set; }
        public string AnswerValue { get; set; } = string.Empty;


        // object navigation properties

        public Question Question { get; set; }
        public Checklist Checklist { get; set; }

    }
}
