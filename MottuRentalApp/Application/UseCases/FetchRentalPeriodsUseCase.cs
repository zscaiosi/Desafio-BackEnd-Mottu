using MottuRentalApp.Application.Ports;
using MottuRentalApp.Domain;
using MottuRentalApp.Application.Exceptions;

namespace MottuRentalApp.Application.UseCases
{
  public class FetchRentalPeriodsUseCase
  {
    private readonly IRentalsPort _rentalsPort;
    public FetchRentalPeriodsUseCase(IRentalsPort rentalsPort)
    {
      this._rentalsPort = rentalsPort;
    }

    public IList<RentalPeriod> Execute()
    {
      return this._rentalsPort.FetchPeriods();
    }
  }
}
