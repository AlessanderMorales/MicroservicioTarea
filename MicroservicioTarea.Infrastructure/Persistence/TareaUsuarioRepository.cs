using System.Collections.Generic;
using Dapper;
using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Infrastructure.Persistence;

namespace MicroservicioTarea.Infrastructure.Repository
{
    public class TareaUsuarioRepository
    {
        private readonly MySqlConnectionSingleton _connection;

        public TareaUsuarioRepository(MySqlConnectionSingleton connection)
        {
            _connection = connection;
        }

        public IEnumerable<TareaUsuario> GetByTareaId(int idTarea)
        {
            using var conn = _connection.CreateConnection();
            const string sql = @"
                SELECT 
                    id, 
                    id_tarea AS IdTarea, 
                    id_usuario AS IdUsuario,
                    fecha_asignacion AS FechaAsignacion, 
                    estado
                FROM Tarea_Usuario
                WHERE id_tarea = @IdTarea AND estado = 1";
            return conn.Query<TareaUsuario>(sql, new { IdTarea = idTarea });
        }

        public void AssignUsers(int idTarea, IEnumerable<int> usuarios)
        {
            using var conn = _connection.CreateConnection();
            conn.Execute("UPDATE Tarea_Usuario SET estado = 0 WHERE id_tarea = @IdTarea;", new { IdTarea = idTarea });

            foreach (var idUsuario in usuarios)
            {
                const string sql = @"
                    INSERT INTO Tarea_Usuario (id_tarea, id_usuario, estado)
                    VALUES (@IdTarea, @IdUsuario, 1)
                    ON DUPLICATE KEY UPDATE estado = 1, fecha_asignacion = NOW();";
                conn.Execute(sql, new { IdTarea = idTarea, IdUsuario = idUsuario });
            }
        }

        public IEnumerable<int> GetTareasByUsuario(int idUsuario)
        {
            using var conn = _connection.CreateConnection();
            const string sql = @"
                SELECT id_tarea
                FROM Tarea_Usuario
                WHERE id_usuario = @IdUsuario AND estado = 1";
            return conn.Query<int>(sql, new { IdUsuario = idUsuario });
        }
    }
}
