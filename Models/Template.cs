using CourseProject.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseProject.Models
{
    public class Template
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Image { get; set; }
        public string? Title { get; set; } = "Untitled";
        public string? Desription { get; set; }
        public AccessTypes AccessType { get; set; }
        [NotMapped]
        public List<Question>? Questions { get; set; }
    }

    public enum AccessTypes
    {
        Public = 0,
        Specified = 1,
        Private = 2
    }
}
