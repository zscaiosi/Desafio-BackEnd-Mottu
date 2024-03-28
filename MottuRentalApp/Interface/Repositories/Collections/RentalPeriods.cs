using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MottuRentalApp.Interface.Repositories.Collections
{
  public class RentalPeriods
  {
    [BsonId]
    [BsonElement("identifier"), BsonRepresentation(BsonType.String)]
    public string? Identifier { get; set; }
    [BsonElement("daysNumber"), BsonRepresentation(BsonType.Int32)]
    public int? DaysNumber { get; set; }
    [BsonElement("dailyPrice"), BsonRepresentation(BsonType.Decimal128)]
    public decimal? DailyPrice { get; set; }
    [BsonElement("periodPrice"), BsonRepresentation(BsonType.Decimal128)]
    public decimal? PeriodPrice { get; set; }
  }
}
