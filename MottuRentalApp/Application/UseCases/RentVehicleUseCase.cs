using MottuRentalApp.Application.Ports;
using MottuRentalApp.Domain;
using MottuRentalApp.Application.Exceptions;
using MottuRentalApp.Application.Facades;

namespace MottuRentalApp.Application.UseCases
{
  public class RentVehicleUseCase
  {
    private readonly IRentalsPort _rentalsPort;
    private readonly IRentalVehiclesFacade _rentalVehiclesFacade;
    private readonly IUsersPort _usersPort;
    private const string FEATURE_NAME = "RENT_VEHICLE";
    private const decimal DAYS_ABOVE_FINE = 50.00M;
    public RentVehicleUseCase(
      IRentalsPort rentalsPort,
      IRentalVehiclesFacade rentalVehiclesFacade,
      IUsersPort usersPort
    )
    {
      this._rentalsPort = rentalsPort;
      this._rentalVehiclesFacade = rentalVehiclesFacade;
      this._usersPort = usersPort;
    }

    public async void ExecuteAsync(string userId, string endTerm)
    {
      var checkUserLicensingTask = CheckUserLicensingAsync(userId);
      var checkUserTask = CheckUserAvailabilityAsync(userId);
      var periodsTask = this._rentalsPort.FetchPeriodsAsync();

      Task.WaitAll(checkUserLicensingTask, checkUserTask, periodsTask);

      Rental rental = this._rentalVehiclesFacade.RentAvailableVehicle(userId, endTerm);
      this._rentalsPort.StartRental(rental);
    }

    private async Task CheckUserLicensingAsync(string userId)
    {
      var user = await this._usersPort.FindUserAsync(userId);
      if (user is null || !user.Documents.Any((doc) => doc.Type == DocumentType.CNH && doc.Category.Equals('A'))) {
        throw new InvalidRentException("USER_UNLICENSED", FEATURE_NAME);
      }
    }

    private async Task CheckUserAvailabilityAsync(string userId)
    {
      var rental = await this._rentalsPort.FindByUserAsync(userId);
      if (rental != null && (rental?.Status == RentalStatus.ACTIVE || rental?.Status == RentalStatus.PENDING)) {
        throw new InvalidRentException("USER_ALREADY_ON_RENT", FEATURE_NAME);
      }
    }

    private decimal CalculateRentalFare(IList<RentalPeriod> periods, string endTerm)
    {
      double totalDays = DateTime.Parse(endTerm).Subtract(DateTime.UtcNow).TotalDays;

      var selectedPeriod = periods.Where((period) => period.DaysNumber <= Math.Floor(totalDays)).LastOrDefault();
      if (selectedPeriod == null) {
        throw new InvalidRentException("NO_PERIOD", FEATURE_NAME);
      }

      if (totalDays == selectedPeriod.DaysNumber) {
        return selectedPeriod.PeriodPrice;
      } else {
        var daysOffset = (decimal) totalDays - selectedPeriod.DaysNumber;

        return daysOffset * DAYS_ABOVE_FINE;
      }
    }
  }
}
