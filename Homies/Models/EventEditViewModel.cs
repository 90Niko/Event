namespace Homies.Models
{
    public class EventEditViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Start { get; set; }

        public string End { get; set; }
        public int TypeId { get; set; }
        public ICollection<TypeViewModel> Types { get; set; }
    }
}
