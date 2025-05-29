using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    [CollectionName("Users")]
    public class User : Base
    {
        [BsonRequired]
        public string Username { get; set; } = string.Empty;

        [BsonRequired]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonRequired]
        public string Role { get; set; } = string.Empty;

        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(Username)) 
                throw new ValidationException("Username can't be null");
            if (string.IsNullOrWhiteSpace(PasswordHash))
                throw new ValidationException("Password hash can't be null");
            if (string.IsNullOrWhiteSpace(Role))
                throw new ValidationException("Role can't be null");
        }
    }
}
