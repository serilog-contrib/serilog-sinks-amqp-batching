
# serilog-sinks-amqp

A Serilog sink that writes AMQP 1.0 messages using the [AMQPNetLite](https://www.nuget.org/packages/AMQPNetLite/) package. 

It has been tested to work with EventHub and RabbitMQ.

## Quick Start

The most basic minimalistic sink initialization is done like this.

```
namespace SerilogAMQPSinkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.AMQP(new AMQPSinkOptions()
                {
                    ConnectionString = configuration["AMQPConnectionString"],
                    SenderLinkName = configuration["AMQPSenderLinkName"],
                    Entity = configuration["AMQPEntity"],
                    PeriodicBatchingSinkOptions = new PeriodicBatchingSinkOptions()
                    {
                        BatchSizeLimit = int.Parse(configuration["AMQPBatchSizeLimit"]),
                        QueueLimit = int.Parse(configuration["AMQPQueueLimit"]),
                        EagerlyEmitFirstEvent = bool.Parse(configuration["AMQPQEagerlyEmitFirstEvent"]),
                        Period = TimeSpan.FromSeconds(int.Parse(configuration["AMQPPeriod"]))
                    }
                })
                .CreateLogger();
            
            Log.Logger.Information("Test");
        }
    }
}
```
## Connection String Format

Use the `ConnectionString` property of `AMQPSinkOptions` to specify the endpoint.

A free hosted and managed RabbitMQ instance can be created at [stackhero](https://www.stackhero.io). Current sink has been tested with RabbitMQ and works following connection string format:
```
amqps://username:passwordH@aileuh.stackhero-network.com:5671
```
The sink has also been tested with EventHuib and works with the same format.

`amqps://username:password=@my-eventhub.servicebus.windows.net:5671`


