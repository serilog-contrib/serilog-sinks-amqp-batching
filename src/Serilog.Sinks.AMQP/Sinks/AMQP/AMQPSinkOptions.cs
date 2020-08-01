using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Formatting.Raw;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AMQP
{
    public class AMQPSinkOptions
    {
        public string SenderLinkName { get; set; }
        public string ConnectionString { get; set; }
        public string Entity { get; set; }
        public string MessagePropertiesGroupId { get; set; }
        public PeriodicBatchingSinkOptions PeriodicBatchingSinkOptions { get; set; }
        public ITextFormatter TextFormatter { get; set; } = new JsonFormatter();
    }
}
