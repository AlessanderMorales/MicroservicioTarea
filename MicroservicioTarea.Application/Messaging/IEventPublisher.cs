using MicroservicioTarea.Domain.Events;

namespace MicroservicioTarea.Application.Messaging
{
    public interface IEventPublisher
    {
        void PublishTareaAsignada(TareaAsignadaEvent evento);
    }
}
