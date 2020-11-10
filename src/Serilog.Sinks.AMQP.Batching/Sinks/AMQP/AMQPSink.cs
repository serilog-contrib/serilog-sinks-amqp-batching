using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AMQP.Batching
{
    public class AMQPSink : IBatchedLogEventSink, IDisposable
    {
        private readonly AMQPSinkOptions _options;
        private Connection _connection;
        private Session _session;
        private SenderLink _senderLink;
        private bool _isRunning;

        public AMQPSink(AMQPSinkOptions options)
        {
            _options = options;
            InitializeConnection();
        }

        public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {

            if (_isRunning)
            {
                return; // Prevent resending same logs when current batch is taking long time
            }

            try
            {
                _isRunning = true;

                var messages = new List<Task>();
                foreach (var logEvent in batch)
                {
                    byte[] body;
                    using (var render = new StringWriter())
                    {
                        _options.TextFormatter.Format(logEvent, render);
                        body = Encoding.UTF8.GetBytes(render.ToString());
                    }

                    var message = new Message
                    {
                        BodySection = new Amqp.Framing.Data() {Binary = body},
                        Properties = new Properties() {GroupId = _options.MessagePropertiesGroupId}
                    };

                    messages.Add(_senderLink.SendAsync(message));
                }

                await Task.WhenAll(messages);

                _isRunning = false;
            }
            catch (AmqpException amqpException)
            {
                if (amqpException.Error.Condition == ErrorCode.ConnectionForced)
                {
                    await _connection.CloseAsync();
                    await _session.CloseAsync();
                    await _senderLink.CloseAsync();
                    InitializeConnection();
                }
                _isRunning = false;

                Log.Logger.Error(amqpException, $"Internal Sink AMQPException. Error condition: {amqpException.Error.Condition}"); // Should get caught by another Sink
                throw;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Internal Sink exception"); // Should get caught by another Sink
                _isRunning = false;
                throw;
            }
        }

        public void InitializeConnection()
        {
            _connection = new Connection(new Address(_options.ConnectionString));
            _session = new Session(_connection);
            _senderLink = new SenderLink(_session, _options.SenderLinkName, _options.Entity);
        }

        public async Task OnEmptyBatchAsync()
        {
            await Task.CompletedTask;
        }

        public async void Dispose()
        {
            try
            {
                await _connection.CloseAsync();
                await _session.CloseAsync();
                await _senderLink.CloseAsync();
            }
            catch
            {
                // ignore exceptions
            }
        }
    }
}
