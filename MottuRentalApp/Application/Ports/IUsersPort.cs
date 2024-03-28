using System;
using MottuRentalApp.Domain;

namespace MottuRentalApp.Application.Ports
{
  public interface IUsersPort
  {
    public User SaveUser(User user);
    public Task<User?> FindUserAsync(string Identifier);
  }
}
