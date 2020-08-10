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
        private readonly Connection _connection;
        private readonly Session _session;
        private readonly SenderLink _senderLink;
        private bool _isRunning;

        public AMQPSink(AMQPSinkOptions options)
        {
            _options = options;

            _connection = new Connection(new Address(options.ConnectionString));
            _session = new Session(_connection);
            _senderLink = new SenderLink(_session, options.SenderLinkName, options.Entity);
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
                        BodySection = new Amqp.Framing.Data() { Binary = body },
                        Properties = new Properties() { GroupId = _options.MessagePropertiesGroupId }
                    };

                    messages.Add(_senderLink.SendAsync(message));
                }

                await Task.WhenAll(messages);

                _isRunning = false;
            }
            catch (Exception)
            {
                _isRunning = false;
                throw;
            }
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
