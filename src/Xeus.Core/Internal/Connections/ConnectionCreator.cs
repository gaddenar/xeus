using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Configuration;
using Omnix.Network;
using Xeus.Core.Internal.Connections.Primitives;
using Xeus.Core.Internal.Primitives;
using Xeus.Messages;

namespace Xeus.Core.Internal.Connections
{
    internal sealed class ConnectionCreator : ServiceBase, ISettings
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BufferPool _bufferPool;
        private readonly TcpConnectionCreator _tcpConnectionCreator;

        private readonly SettingsDatabase _settings;

        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly object _lockObject = new object();

        public ConnectionCreator(string configPath, BufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _tcpConnectionCreator = new TcpConnectionCreator(configPath, bufferPool);

            _settings = new SettingsDatabase(Path.Combine(configPath, nameof(ConnectionCreator)));
        }

        public void SetOptions(ConnectionCreatorOptions options)
        {
            lock (_lockObject)
            {
                _tcpConnectionCreator.SetTcpConnectOptions(options.TcpConnectOptions);
                _tcpConnectionCreator.SetTcpAcceptOptions(options.TcpAcceptOptions);
            }
        }

        public async ValueTask<Cap?> ConnectAsync(OmniAddress address, CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return null;
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return null;
            }

            Cap? result;

            if ((result = await _tcpConnectionCreator.ConnectAsync(address, token)) != null)
            {
                return result;
            }

            return null;
        }

        public async ValueTask<(Cap?, OmniAddress?)> AcceptAsync(CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return default;
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return default;
            }

            (Cap?, OmniAddress?) result;

            if ((result = await _tcpConnectionCreator.AcceptAsync(token)) != default)
            {
                return result;
            }

            return default;
        }

        protected override async ValueTask OnInitializeAsync()
        {
        }

        protected override async ValueTask OnStartAsync()
        {
            this.StateType = ServiceStateType.Starting;

            await _tcpConnectionCreator.StartAsync();

            this.StateType = ServiceStateType.Running;
        }

        protected override async ValueTask OnStopAsync()
        {
            this.StateType = ServiceStateType.Stopping;

            await _tcpConnectionCreator.StopAsync();

            this.StateType = ServiceStateType.Stopped;
        }

        public async ValueTask LoadAsync()
        {
            await _tcpConnectionCreator.LoadAsync();
        }

        public async ValueTask SaveAsync()
        {
            await _tcpConnectionCreator.SaveAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tcpConnectionCreator.Dispose();
            }
        }
    }
}
