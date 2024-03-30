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

    public async Task<Rental> ExecuteAsync(string userId, string periodId)
    {
      var checkUserLicensingTask = CheckUserLicensingAsync(userId);
      var checkUserTask = CheckUserAvailabilityAsync(userId);
      var periodTask = this._rentalsPort.FindPeriodAsync(periodId);

      try
      {
        await Task.WhenAll(checkUserLicensingTask, checkUserTask, periodTask);

        var selectedPeriod = periodTask.Result;
        if (selectedPeriod is null) {
          throw new InvalidRentException($"Could not find period {periodId} in {periodTask.AsyncState}", FEATURE_NAME);
        }

        Rental rental = this._rentalVehiclesFacade.RentAvailableVehicle(
          userId,
          DateTime.UtcNow.AddDays(1 + selectedPeriod.DaysNumber!).ToString("yyyy-MM-dd")
        );
        rental.TotalFare = selectedPeriod.PeriodPrice;

        return this._rentalsPort.StartRental(rental);
      }
      catch(Exception exc)
      {
        throw new InvalidRentException(exc.Message, FEATURE_NAME);
      }
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
  }
}
