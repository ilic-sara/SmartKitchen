using Models.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Ingredient
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        public string Amount { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        [BsonIgnoreIfNull]
        public UnitType? UnitType { get; set; }
    }
}
