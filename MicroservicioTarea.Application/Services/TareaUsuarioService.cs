using System.Collections.Generic;
using MicroservicioTarea.Infrastructure.Repository;

namespace MicroservicioTarea.Application.Services
{
    public class TareaUsuarioService
    {
        private readonly TareaUsuarioRepository _repo;

        public TareaUsuarioService(TareaUsuarioRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<dynamic> GetByTareaId(int idTarea) =>
            _repo.GetByTareaId(idTarea);

        public void AssignUsers(int idTarea, IEnumerable<int> usuarios) =>
            _repo.AssignUsers(idTarea, usuarios);
    }
}
