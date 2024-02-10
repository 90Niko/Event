using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Homies.Data
{
    [Comment("This class represents a type.")]
    public class Type
    {
        [Key]
        [Comment("The unique identifier of the type.")]
        public int Id { get; set; }

        [Required]
        [MaxLength(DataConstants.TypeNameMaxLength)]
        [Comment("The name of the type.")]
        public string Name { get; set; }=string.Empty;

        [Comment("The events of the type.")]
        public ICollection<Event> Events { get; set; }=new List<Event>();
    }
}
