using MottuRentalApp.Application.Ports;
using MottuRentalApp.Application.Facades;
using MottuRentalApp.Domain;
using Moq;
using MottuRentalApp.Application.UseCases;
using MottuRentalApp.Application.Exceptions;

namespace MottuRentalApp.Tests.Application.UseCases
{
  public class RentVehicleUseCaseTest
  {
    private readonly Mock<IRentalsPort> _rentalsPort = new Mock<IRentalsPort>();
    private readonly Mock<IRentalVehiclesFacade> _rentalVehiclesFacade = new Mock<IRentalVehiclesFacade>();
    private readonly Mock<IUsersPort> _usersPort = new Mock<IUsersPort>();
    private readonly RentVehicleUseCase _underTest;
    private const string FEATURE_NAME = "RENT_VEHICLE";

    public RentVehicleUseCaseTest()
    {
      this._underTest = new RentVehicleUseCase(
        this._rentalsPort.Object,
        this._rentalVehiclesFacade.Object,
        this._usersPort.Object
      );
    }

    [Fact]
    public async Task ShouldRentVehicleWhenAvailable()
    {
      string userId = "123";
      string vehicleId = "RHZ2G59";
      string periodId = Guid.NewGuid().ToString();
      string pastEndTerm = "2024-03-26";
      string endTerm = "2024-04-05";
      User? foundUser = new User(
        "name",
        "1992-11-29",
        UserType.Courrier,
        [
          new Document() { UserId = userId, Number = "123", Type = DocumentType.CNH, Category = 'A' },
          new Document() { UserId = userId, Number = "12345", Type = DocumentType.CNPJ }
        ]
      );
      RentalPeriod expectedPeriod = new RentalPeriod(periodId, 7, 30.00M);
      Rental doneRental = new Rental(userId, vehicleId, pastEndTerm) { StartTerm = DateTime.Parse(pastEndTerm) };
      Rental expectedRental = new Rental(userId, vehicleId, endTerm) { TotalFare = expectedPeriod.PeriodPrice };

      this._usersPort.Setup((port) => port.FindUserAsync(userId)).ReturnsAsync(foundUser);
      this._rentalsPort.Setup((port) => port.FindByUserAsync(userId)).ReturnsAsync(doneRental);
      this._rentalsPort.Setup((port) => port.FindPeriodAsync(periodId).Result).Returns(() => expectedPeriod);
      this._rentalVehiclesFacade.Setup((f) => f.RentAvailableVehicle(userId, endTerm)).Returns(expectedRental);
      this._rentalsPort.Setup((port) => port.StartRental(expectedRental)).Returns(expectedRental);

      var result = await this._underTest.ExecuteAsync(userId, endTerm);

      Assert.NotNull(result);
    }

    [Fact]
    public async Task ShouldThrowWhenUnavailable()
    {
      string userId = "123";
      string vehicleId = "RHZ2G59";
      string periodId = Guid.NewGuid().ToString();
      string pastEndTerm = "2024-03-26";
      string endTerm = "2024-04-05";
      User? foundUser = new User(
        "name",
        "1992-11-29",
        UserType.Courrier,
        [
          new Document() { UserId = userId, Number = "123", Type = DocumentType.CNH, Category = 'A' },
          new Document() { UserId = userId, Number = "12345", Type = DocumentType.CNPJ }
        ]
      );
      RentalPeriod expectedPeriod = new RentalPeriod(periodId, 7, 30.00M);
      Rental doneRental = new Rental(userId, vehicleId, pastEndTerm) { StartTerm = DateTime.Parse(pastEndTerm) };
      Rental expectedRental = new Rental(userId, vehicleId, endTerm) { TotalFare = expectedPeriod.PeriodPrice };

      this._usersPort.Setup((port) => port.FindUserAsync(userId)).ReturnsAsync(foundUser);
      this._rentalsPort.Setup((port) => port.FindByUserAsync(userId))
        .ThrowsAsync(new InvalidRentException("USER_ALREADY_ON_RENT", FEATURE_NAME));
      this._rentalsPort.Setup((port) => port.FindPeriodAsync(periodId)).ReturnsAsync(expectedPeriod);
      this._rentalVehiclesFacade.Setup((f) => f.RentAvailableVehicle(userId, endTerm)).Returns(expectedRental);
      this._rentalsPort.Setup((port) => port.StartRental(expectedRental)).Returns(expectedRental);

      await Assert.ThrowsAsync<InvalidRentException>(async () => await this._underTest.ExecuteAsync(userId, endTerm));
    }
  }
}
