using Dapper;
using System.Data;
using Website.Persistence;

namespace Website.Test
{
    public abstract class TestBase : IAsyncLifetime
    {
        protected readonly DbContext _context;
        protected readonly IDbConnection _connection;

        protected TestBase()
        {
            var factory = new TestDbConnectionFactory();
            _connection = factory.CreateConnection();
            _context = new DbContext(factory);
            SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());
        }

        public async Task InitializeAsync()
        {
            await _context.InitialiseDatabase(CancellationToken.None);

        }


        public async Task DisposeAsync()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}