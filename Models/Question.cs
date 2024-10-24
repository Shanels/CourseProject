using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string Label { get; set; } = string.Empty;
        public QestionTypes Type { get; set; }

        public enum QestionTypes
        {
            [Display (Name = "String")]
            SingleLine = 0,
            [Display(Name = "Text")]
            MultiLine = 1,
            [Display(Name = "Number")]
            PositiveIntegers = 2,
            Checkbox = 3
        }
    }
}