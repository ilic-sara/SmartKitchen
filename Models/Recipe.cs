using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    [CollectionName("Recipes")]
    public class Recipe : Base
    {
        [BsonRequired]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 30 characters")]
        public string Name { get; set; } = string.Empty;

        [BsonRequired]
        [Required(ErrorMessage = "Picture Url is required")]
        [Url(ErrorMessage = "Picture Url must be a valid URL")]
        public string PictureUrl { get; set; } = string.Empty;

        [BsonRequired]
        [Required(ErrorMessage = "Number of servings is required")]
        public int Servings { get; set; } = 1;

        [BsonRequired]
        [Required(ErrorMessage = "Prep time is required")]
        public string PrepTime { get; set; } = string.Empty;

        public List<Category> Categories { get; set; } = [];
        public List<Cuisine> Cuisines { get; set; } = [];
        public List<Diet> Diets { get; set; } = [];
        public List<Method> Methods { get; set; } = [];

        [BsonRequired]
        [Required(ErrorMessage = "Ingredients are required")]
        public List<Ingredient> Ingredients { get; set; } = [];

        [BsonRequired]
        [Required(ErrorMessage = "Steps are required")]
        public List<string> Steps { get; set; } = [];

        [BsonRequired]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
