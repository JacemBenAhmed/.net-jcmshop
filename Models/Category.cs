using System.ComponentModel.DataAnnotations;

namespace projTP.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }


}
