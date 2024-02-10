using Homies.Data;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.ComponentModel.DataAnnotations;

namespace Homies.Models
{
    public class EventFormViewModel
    {
        [Required]
        [StringLength(DataConstants.EventNameMaxLength, MinimumLength = DataConstants.EventDescriptionMinLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(DataConstants.EventDescriptionMaxLength, MinimumLength = DataConstants.EventDescriptionMinLength)]
        public string Description { get; set; }

        [Required]
        public string Start { get; set; } = string.Empty;

        [Required]
        public string End { get; set; } = string.Empty;

        [Required]
        public int TypeId { get; set; }

        public ICollection<TypeViewModel> Types { get; set; } = new List<TypeViewModel>();
    }
}
