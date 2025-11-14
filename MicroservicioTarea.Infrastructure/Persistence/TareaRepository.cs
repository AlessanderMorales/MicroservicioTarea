using Dapper;
using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Domain.Interfaces;
using MicroservicioTarea.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            return conn.Query<Tarea>(
                @"SELECT 
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
                  WHERE estado = 1
                  ORDER BY id_tarea DESC");
        }

        public Tarea GetById(int id)
        {
            using var conn = _connection.CreateConnection();

            return conn.QueryFirstOrDefault<Tarea>(
                @"SELECT 
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
                WHERE id_tarea = @Id;",
                new { Id = id });
        }

        public void Add(Tarea entity)
        {
            using var conn = _connection.CreateConnection();

            conn.Execute(
                @"INSERT INTO Tareas (titulo, descripcion, prioridad, id_proyecto, status)
                  VALUES (@Titulo, @Descripcion, @Prioridad, @IdProyecto, @Status);",
                entity);
        }

        public void Update(Tarea entity)
        {
            using var conn = _connection.CreateConnection();

            conn.Execute(
                @"UPDATE Tareas
                  SET titulo = @Titulo,
                      descripcion = @Descripcion,
                      prioridad = @Prioridad,
                      id_proyecto = @IdProyecto,
                      status = @Status
                  WHERE id_tarea = @IdTarea;",
                entity);
        }

        public void Delete(int id)
        {
            using var conn = _connection.CreateConnection();

            conn.Execute(
                @"UPDATE Tareas SET estado = 0 WHERE id_tarea = @Id;",
                new { Id = id });
        }
    }
}

