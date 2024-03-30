using MottuRentalApp.Domain;
using MottuRentalApp.Interface.Repositories;

namespace MottuRentalApp.Application.Ports
{
  public interface IRentalsPort
  {
    public Rental StartRental(Rental rental);
    public Task<Rental?> FindByUserAsync(string userId);
    public Rental? FindByVehiclePlate(string licensePlate);
    public IList<RentalPeriod> FetchPeriods();
    public Task<RentalPeriod?> FindPeriodAsync(string identifier);
    public IList<Rental> FetchOngoing();
    public Task PatchTermAndFare(PatchRentalDto dto);
  }
}
