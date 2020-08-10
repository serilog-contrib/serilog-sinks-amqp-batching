using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AMQP.Batching
{
    public static class LoggerConfigurationAMQPExtension
    {
        public static LoggerConfiguration AMQP(this LoggerSinkConfiguration loggerSinkConfiguration, AMQPSinkOptions amqpOptions, LogEventLevel restrictedTiMinimumLevel = LogEventLevel.Verbose)
        {
            var amqpSink = new AMQPSink(amqpOptions);

            var periodicBatchingOptions = amqpOptions.PeriodicBatchingSinkOptions;

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = periodicBatchingOptions.BatchSizeLimit,
                Period = periodicBatchingOptions.Period,
                EagerlyEmitFirstEvent = periodicBatchingOptions.EagerlyEmitFirstEvent,
                QueueLimit = periodicBatchingOptions.QueueLimit
            };

            var batchingSink = new PeriodicBatchingSink(amqpSink, batchingOptions);

            return loggerSinkConfiguration.Sink(batchingSink, restrictedTiMinimumLevel);
        }
    }
}