using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Domain.Interfaces;
using MicroservicioTarea.Infrastructure.Repository;

namespace MicroservicioTarea.Application.Services
{
    public class TareaService
    {
        private readonly IRepository<Tarea> _repo;
        private readonly TareaUsuarioRepository _tareaUsuarioRepository;

        public TareaService(IRepository<Tarea> repo, TareaUsuarioRepository tareaUsuarioRepository)
        {
            _repo = repo;
            _tareaUsuarioRepository = tareaUsuarioRepository;
        }

        public IEnumerable<Tarea> GetAll() =>
            _repo.GetAll().Where(t => t.Estado == 1).ToList();

        public Tarea GetById(int id) => _repo.GetById(id);

        public void Add(Tarea t) => _repo.Add(t);

        public void Update(Tarea t) => _repo.Update(t);

        public void Delete(int id) => _repo.Delete(id);

        public IEnumerable<Tarea> GetByUsuarioAsignado(int idUsuario)
        {
            var tareasIds = _tareaUsuarioRepository.GetTareasByUsuario(idUsuario).ToList();

            if (!tareasIds.Any())
                return Enumerable.Empty<Tarea>();

            return _repo.GetAll()
                        .Where(t => tareasIds.Contains(t.IdTarea) && t.Estado == 1)
                        .ToList();
        }

        public IEnumerable<Tarea> GetByProyecto(int idProyecto) =>
            _repo.GetAll()
                 .Where(t => t.IdProyecto == idProyecto && t.Estado == 1)
                 .ToList();
    }
}
