using System.Data.SQLite;
using System.Threading.Tasks;

namespace Hurricane.Model.Data
{
    interface IDataProvider
    {
        Task CreateTables(SQLiteConnection connection);
        Task Load(SQLiteConnection connection);
    }
}
