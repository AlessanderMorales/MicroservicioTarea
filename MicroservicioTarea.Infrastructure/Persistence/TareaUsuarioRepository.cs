using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            return conn.Query<TareaUsuario>(
                @"SELECT id, id_tarea AS IdTarea, id_usuario AS IdUsuario,
                         fecha_asignacion AS FechaAsignacion, estado
                  FROM Tarea_Usuario
                  WHERE id_tarea = @IdTarea AND estado = 1",
                new { IdTarea = idTarea });
        }

        public void AssignUsers(int idTarea, IEnumerable<int> usuarios)
        {
            using var conn = _connection.CreateConnection();

            conn.Execute("UPDATE Tarea_Usuario SET estado = 0 WHERE id_tarea = @IdTarea;",
                new { IdTarea = idTarea });

            foreach (var idUsuario in usuarios)
            {
                conn.Execute(
                    @"INSERT INTO Tarea_Usuario (id_tarea, id_usuario, estado)
                      VALUES (@IdTarea, @IdUsuario, 1)
                      ON DUPLICATE KEY UPDATE estado = 1, fecha_asignacion = NOW();",
                    new { IdTarea = idTarea, IdUsuario = idUsuario });
            }
        }

        public IEnumerable<int> GetTareasByUsuario(int idUsuario)
        {
            using var conn = _connection.CreateConnection();

            return conn.Query<int>(
                @"SELECT id_tarea
          FROM Tarea_Usuario
          WHERE id_usuario = @IdUsuario AND estado = 1",
                new { IdUsuario = idUsuario });
        }
    }
}

