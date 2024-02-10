using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Type=Homies.Data.Type;

namespace Homies.Data
{
    [Comment("This class represents an event.")]
    public class Event
    {
        [Key]
        [Comment("The unique identifier of the event.")]
        public int Id { get; set; }

        [Required]
        [MaxLength(DataConstants.EventNameMaxLength)]
        public string Name { get; set; }=string.Empty;

        [Required]
        [MaxLength(DataConstants.EventDescriptionMaxLength)]
        public string Description { get; set; }=string.Empty;

        [Required]
        public string OrganiserId { get; set; }=string.Empty;

        [Required]
        [ForeignKey(nameof(OrganiserId))]
        public IdentityUser Organiser { get; set; }=null!;

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        [ForeignKey(nameof(TypeId))]
        public Type Type { get; set; }=null!;


        public ICollection<EventParticipant> EventsParticipants { get; set; }=new List<EventParticipant>();
    }
}
