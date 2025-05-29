using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    [CollectionName("Methods")]
    public class Method : Base
    {
        [BsonRequired]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 30 characters")]
        public string Name { get; set; } = string.Empty;

        [BsonRequired]
        [Required(ErrorMessage = "Picture Url is required")]
        [Url(ErrorMessage = "Picture Url must be a valid URL")]
        public string PictureUrl { get; set; } = string.Empty;
    }
}
