using MicroservicioTarea.Application.Services;
using MicroservicioTarea.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace MicroservicioTarea.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TareaController : ControllerBase
    {
        private readonly TareaService _service;
        private readonly TareaUsuarioService _usuarioService;

        public TareaController(TareaService service, TareaUsuarioService usuarioService)
        {
            _service = service;
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(int id) => Ok(_service.GetById(id));

        [HttpPost]
        public IActionResult Create(Tarea t)
        {
            _service.Add(t);
            return Ok("Tarea creada.");
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Tarea t)
        {
            t.IdTarea = id;
            _service.Update(t);
            return Ok("Tarea actualizada.");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok("Tarea eliminada.");
        }

        // Usuarios asociados
        [HttpGet("{id}/usuarios")]
        public IActionResult GetUsuarios(int id)
            => Ok(_usuarioService.GetByTareaId(id));

        [HttpPost("{id}/usuarios")]
        public IActionResult AssignUsers(int id, List<int> usuarios)
        {
            _usuarioService.AssignUsers(id, usuarios);
            return Ok("Usuarios asignados.");
        }
    }
}

