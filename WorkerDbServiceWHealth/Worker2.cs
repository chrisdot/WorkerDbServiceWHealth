using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDbServiceWHealth
{
	public class Worker2: BackgroundService
	{
		private readonly ILogger<Worker2> _logger;
		private readonly IHostApplicationLifetime _hostApplicationLifetime;

		public Worker2(ILogger<Worker2> logger, IHostApplicationLifetime hostApplicationLifetime)
		{
			_logger = logger;
			_hostApplicationLifetime = hostApplicationLifetime;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker2 running at: {time}", DateTimeOffset.Now);
				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}