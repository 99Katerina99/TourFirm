using Microsoft.Data.Sqlite;
using AppDb = TravelAgency.Data.DbContext;
using TravelAgency.Data.Repositories;

namespace TravelAgency.Tests.Fixtures;

public class RepositoryTestsFixture : DatabaseFixture
{
    public EfRepository<Core.Entities.Tour> EfTourRepo => new(DbContext);

    // Передаём строку подключения из базового класса
    public SqlTourRepository SqlTourRepo => new(_connection.ConnectionString);
}