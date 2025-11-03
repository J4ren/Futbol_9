using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.Threading;

namespace Futbol_9.Datos
{
    public static class ConexionSqlite
    {
        private static SQLiteAsyncConnection? _conexion;
        private static readonly SemaphoreSlim _semaforo = new(1, 1);

        public static async Task<SQLiteAsyncConnection> ObtenerConexionAsync()
        {
            if (_conexion is not null) return _conexion;

            await _semaforo.WaitAsync();
            try
            {
                if (_conexion is null)
                {
                    var rutaDb = Path.Combine(FileSystem.AppDataDirectory, "futbol9.db3");
                    _conexion = new SQLiteAsyncConnection(
                        rutaDb,
                        SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache
                    );
                }
                return _conexion;
            }
            finally
            {
                _semaforo.Release();
            }
        }
    }
}
