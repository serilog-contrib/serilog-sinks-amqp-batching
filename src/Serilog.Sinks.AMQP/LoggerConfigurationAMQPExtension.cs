using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AMQP
{
    public static class LoggerConfigurationAMQPExtension
    {
        public static LoggerConfiguration AMQP(this LoggerSinkConfiguration loggerSinkConfiguration, AMQPSinkOptions amqpoptions)
        {
            var amqpSink = new AMQPSink(amqpoptions);

            var periodicBatchingOptions = amqpoptions.PeriodicBatchingSinkOptions;

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = periodicBatchingOptions.BatchSizeLimit,
                Period = periodicBatchingOptions.Period,
                EagerlyEmitFirstEvent = periodicBatchingOptions.EagerlyEmitFirstEvent,
                QueueLimit = periodicBatchingOptions.QueueLimit
            };

            var batchingSink = new PeriodicBatchingSink(amqpSink, batchingOptions);

            return loggerSinkConfiguration.Sink(batchingSink);
        }
    }
}