using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation property - THIS WAS MISSING
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}