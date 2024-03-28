using MottuRentalApp.Application.Ports;
using MottuRentalApp.Interface.Gateways.Interfaces;
using MottuRentalApp.Interface.Repositories.Collections;
using MongoDB.Driver;
using MottuRentalApp.Domain;

namespace MottuRentalApp.Interface.Repositories
{
  public class UsersRepository : IUsersPort
  {
    private readonly IMongoService _mongoDBService;
    private readonly IMongoCollection<Users> _users;
    public UsersRepository(IMongoService mongoService)
    {
      this._mongoDBService = mongoService;
      this._users = this._mongoDBService.GetDbConnection().GetCollection<Users>("users") ??
        throw new IOException("BAD_DATABASE_CONNECTION");
    }

    public User SaveUser(User user)
    {
      this._users.InsertOne(new Users() {
        Identifier = user.Identifier,
        BirthDate = user.BirthDate,
        UserType = (int) user.UserType,
        Documents = user.Documents
      });

      return user;
    }

    public async Task<User?> FindUserAsync(string identifier)
    {
      var filter = Builders<Users>.Filter.Eq(u => u.Identifier, identifier);

      var cursor = await this._users.FindAsync(filter);
      var doc = await cursor.FirstOrDefaultAsync();

      if (doc == null) {
        return null;
      } else {
        return new User(doc.Name!, doc.BirthDate!, (UserType) doc.UserType!, doc.Documents!)
        {
          Identifier = doc.Identifier!
        };
      }
    }
  }
}