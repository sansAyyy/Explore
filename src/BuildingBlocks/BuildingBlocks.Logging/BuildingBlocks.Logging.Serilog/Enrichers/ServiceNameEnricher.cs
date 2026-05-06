using Serilog.Core;
using Serilog.Events;

namespace BuildingBlocks.Logging.Serilog.Enrichers
{
    public class ServiceNameEnricher : ILogEventEnricher
    {
        private readonly string _serviceName;

        public ServiceNameEnricher(string serviceName)
        {
            _serviceName = serviceName;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
        {
            logEvent.AddPropertyIfAbsent(
                factory.CreateProperty("ServiceName", _serviceName));
        }
    }
}

