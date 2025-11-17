using Dapper;
using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Domain.Interfaces;
using MicroservicioTarea.Infrastructure.Persistence;

namespace MicroservicioTarea.Infrastructure.Repository
{
    public class TareaRepository : IRepository<Tarea>
    {
        private readonly MySqlConnectionSingleton _connection;

        public TareaRepository(MySqlConnectionSingleton connection)
        {
            _connection = connection;
        }

        public IEnumerable<Tarea> GetAll()
        {
            using var conn = _connection.CreateConnection();

            const string sql = @"SELECT 
                                    id_tarea AS IdTarea,
                                    titulo,
                                    descripcion,
                                    prioridad,
                                    estado,
                                    fechaRegistro,
                                    ultimaModificacion,
                                    id_proyecto AS IdProyecto,
                                    id_usuario_asignado AS IdUsuarioAsignado,
                                    status
                                FROM Tareas";

            return conn.Query<Tarea>(sql);
        }

        public Tarea GetById(int id)
        {
            using var conn = _connection.CreateConnection();

            const string sql = @"SELECT 
                                    id_tarea AS IdTarea,
                                    titulo,
                                    descripcion,
                                    prioridad,
                                    estado,
                                    fechaRegistro,
                                    ultimaModificacion,
                                    id_proyecto AS IdProyecto,
                                    id_usuario_asignado AS IdUsuarioAsignado,
                                    status
                                FROM Tareas
                                WHERE id_tarea = @Id";

            return conn.QueryFirstOrDefault<Tarea>(sql, new { Id = id });
        }

        public void Add(Tarea t)
        {
            using var conn = _connection.CreateConnection();

            const string sql = @"INSERT INTO Tareas
                                (titulo, descripcion, prioridad, id_proyecto, status, estado)
                                VALUES (@Titulo, @Descripcion, @Prioridad, @IdProyecto, @Status, @Estado)";

            conn.Execute(sql, t);
        }

        public void Update(Tarea t)
        {
            using var conn = _connection.CreateConnection();

            const string sql = @"UPDATE Tareas SET
                                    titulo = @Titulo,
                                    descripcion = @Descripcion,
                                    prioridad = @Prioridad,
                                    id_proyecto = @IdProyecto,
                                    status = @Status,
                                    estado = @Estado
                                WHERE id_tarea = @IdTarea";

            conn.Execute(sql, t);
        }

        public void Delete(int id)
        {
            using var conn = _connection.CreateConnection();

            const string sql = "DELETE FROM Tareas WHERE id_tarea = @Id";

            conn.Execute(sql, new { Id = id });
        }

        public IEnumerable<Tarea> GetByProyecto(int idProyecto)
        {
            using var conn = _connection.CreateConnection();

            return conn.Query<Tarea>(
                @"SELECT *
          FROM Tareas
          WHERE id_proyecto = @IdProyecto
            AND estado = 1",
                new { IdProyecto = idProyecto }
            );
        }
    }
}
