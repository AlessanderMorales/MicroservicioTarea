using MicroservicioTarea.Domain.Entities;
using MicroservicioTarea.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroservicioTarea.Application.Services
{
    public class TareaService
    {
        private readonly IRepository<Tarea> _repo;

        public TareaService(IRepository<Tarea> repo)
        {
            _repo = repo;
        }

        public IEnumerable<Tarea> GetAll() => _repo.GetAll();
        public Tarea GetById(int id) => _repo.GetById(id);
        public void Add(Tarea t) => _repo.Add(t);
        public void Update(Tarea t) => _repo.Update(t);
        public void Delete(int id) => _repo.Delete(id);
    }
}

