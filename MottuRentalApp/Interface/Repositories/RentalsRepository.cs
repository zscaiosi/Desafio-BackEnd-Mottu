using MongoDB.Driver;
using MottuRentalApp.Application.Ports;
using MottuRentalApp.Domain;
using MottuRentalApp.Interface.Gateways.Interfaces;
using MottuRentalApp.Interface.Repositories.Collections;

namespace MottuRentalApp.Interface.Repositories
{
  public class RentalsRepository : IRentalsPort
  {
    private readonly IMongoService _mongoDBService;
    private readonly IMongoCollection<Rentals> _rentals;
    private readonly IMongoCollection<RentalPeriods> _rentalPeriods;

    public RentalsRepository(IMongoService mongoService)
    {
      this._mongoDBService = mongoService;
      this._rentals = this._mongoDBService.GetDbConnection().GetCollection<Rentals>("rentals");
      this._rentalPeriods = this._mongoDBService.GetDbConnection().GetCollection<RentalPeriods>("rentalPeriods");
    }

    public Rental StartRental(Rental rental)
    {
      this._rentals.InsertOne(new Rentals() {
        Identifier = rental.Identifier,
        UserId = rental.UserId,
        VehicleId = rental.VehicleId,
        StartTerm = rental.StartTerm,
        EndTerm = rental.EndTerm,
        Status = rental.Status.ToString(),
        TotalFare = rental.TotalFare
      });

      return rental;
    }

    public async Task<Rental?> FindByUserAsync(string userId)
    {
      var filter = Builders<Rentals>.Filter.Eq(r => r.UserId, userId);

      var cursor = await this._rentals.FindAsync(filter);
      var doc = await cursor.FirstOrDefaultAsync();

      if (doc == null) {
        return null;
      } else {
        return new Rental(doc.UserId!, doc.VehicleId!, doc.EndTerm.HasValue ? doc.EndTerm.Value.ToString("yyyy-MM-dd") : "")
        {
          Identifier = doc.Identifier!
        };
      }
    }

    public Rental? FindByVehiclePlate(string licensePlate)
    {
      var filter = Builders<Rentals>.Filter.Eq(r => r.VehicleId, licensePlate);

      var doc = this._rentals.Find(filter).FirstOrDefault();

      if (doc == null) {
        return null;
      } else {
        return new Rental(doc.UserId!, doc.VehicleId!, doc.EndTerm.HasValue ? doc.EndTerm.Value.ToString("yyyy-MM-dd") : "")
        {
          Identifier = doc.Identifier!
        };
      }
    }

    public async Task<IList<RentalPeriod>> FetchPeriodsAsync()
    {
      var filter = Builders<RentalPeriods>.Filter.Gte(rp => rp.DaysNumber, 1);

      var cursor = await this._rentalPeriods.FindAsync(filter);
      var docs = await cursor.ToListAsync();

      if (docs == null) {
        return [];
      } else {
        return docs
          .Select(doc => new RentalPeriod(doc.Identifier!, doc.DaysNumber ?? 0, doc.DailyPrice ?? 0M))
            .ToList();
      }
    }

    public async Task<RentalPeriod?> FindPeriodAsync(string identifier)
    {
      var filter = Builders<RentalPeriods>.Filter.Gte(rp => rp.Identifier, identifier);

      var cursor = await this._rentalPeriods.FindAsync(filter);
      var docs = await cursor.ToListAsync();

      if (docs == null) {
        return null;
      } else {
        return docs
          .Select(doc => new RentalPeriod(doc.Identifier!, doc.DaysNumber ?? 0, doc.DailyPrice ?? 0M)).First();
      }

    }

    public IList<Rental> FetchOngoing()
    {
      var pendingFilter = Builders<Rentals>.Filter.Eq(r => r.Status, RentalStatus.PENDING.ToString());
      var activeFilter = Builders<Rentals>.Filter.Eq(r => r.Status, RentalStatus.ACTIVE.ToString());
      var filter = Builders<Rentals>.Filter.Or(pendingFilter, activeFilter);

      var docs = this._rentals.Find(filter).ToList();

      return docs.Select(doc => new Rental(doc.UserId!, doc.VehicleId!, doc.EndTerm.HasValue ? doc.EndTerm.Value.ToString("yyyy-MM-dd") : "")
      {
        Identifier = doc.Identifier!
      }).ToList();
    }
  }
}