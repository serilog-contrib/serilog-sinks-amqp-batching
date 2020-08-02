using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AMQP
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
            if (_isRunning) return; // Prevent resending same logs when current batch is taking long time

            _isRunning = true;

            var messages = new List<Message>();
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

                messages.Add(message);
            }

            await Task.WhenAll(messages.Select(m => _senderLink.SendAsync(m)));

            _isRunning = false;
        }

        public async Task OnEmptyBatchAsync()
        {
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            try
            {
                _connection.Close();
                _session.Close();
                _senderLink.Close();
            }
            catch
            {
                // ignore exceptions
            }
        }
    }
}
