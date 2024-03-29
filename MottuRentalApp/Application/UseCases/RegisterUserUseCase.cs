using MottuRentalApp.Domain;
using MottuRentalApp.Application.Ports;

namespace MottuRentalApp.Application.UseCases
{
  public class RegisterUserUseCase(IUsersPort usersPort)
  {
    private readonly IUsersPort _usersPort = usersPort;

    public User Execute(RegisterUserDto dto)
    {
      CheckDto(dto);

      return this._usersPort.SaveUser(
        new User(dto.Name, dto.BirthDate, (UserType) dto.Type, dto.Documents)
      );
    }

    private void CheckDto(RegisterUserDto dto)
    {
      if (areFieldsInvalid(dto) || areRequiredDocumentsMissing(dto))
      {
        throw new ArgumentException("BAD_PARAMS");
      }
    }

    private bool areFieldsInvalid(RegisterUserDto dto)
    {
      return string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.BirthDate) || dto.Documents.Count < 1;
    }

    private bool areRequiredDocumentsMissing(RegisterUserDto dto)
    {
      dto.Type = dto.Type < 1 || dto.Type > 2 ? 2 : 1;

      if (dto.Type == 1) {
        return dto.Documents.Any(doc => doc.Type == DocumentType.CPF);
      } else {
        return dto.Documents.Select(doc => doc.Type == DocumentType.CNPJ || doc.Type == DocumentType.CNH).Count() == 2;
      }
    }
  }
}
