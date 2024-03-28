using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MottuRentalApp.Domain;

namespace MottuRentalApp.Interface.Repositories.Collections
{
  public class Users
  {
    [BsonId]
    [BsonElement("identifier"), BsonRepresentation(BsonType.String)]
    public string? Identifier { get; set; }
    [BsonElement("name"), BsonRepresentation(BsonType.String)]
		public string? Name { get; }
    [BsonElement("birthDate"), BsonRepresentation(BsonType.String)]
    public string? BirthDate { get; set; }
    [BsonElement("userType"), BsonRepresentation(BsonType.String)]
    public int? UserType { get; set; }
    [BsonElement("documents"), BsonRepresentation(BsonType.Array)]
    public IList<Document>? Documents { get; set; }
  }
}
