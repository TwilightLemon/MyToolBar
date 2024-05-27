using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyToolBar.Services
{
    internal class ApplicationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var mainWindow = _serviceProvider.GetRequiredService<AppBarWindow>();
            mainWindow.Show();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            App.Current.Shutdown();

            return Task.CompletedTask;
        }
    }
}
