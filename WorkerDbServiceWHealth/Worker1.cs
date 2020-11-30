using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;

namespace testWorkerService
{
	public class Worker1 : BackgroundService
	{
		private readonly ILogger<Worker1> _logger;
		private readonly IHostApplicationLifetime _hostApplicationLifetime;

		public Worker1(ILogger<Worker1> logger, IHostApplicationLifetime hostApplicationLifetime)
		{
			_logger = logger;
			_hostApplicationLifetime = hostApplicationLifetime;

			hostApplicationLifetime.ApplicationStarted.Register(() => ApplicationStarted());
			hostApplicationLifetime.ApplicationStopping.Register(() => ApplicationStopping());
			hostApplicationLifetime.ApplicationStopped.Register(() => ApplicationStopped());

			if (WindowsServiceHelpers.IsWindowsService() == true)
			{
				Console.WriteLine("Running as a windows service");
			}
			else
			{
				//...
			}

		}

		private void ApplicationStarted()
		{
			//whole host has been started (means all registered services have been started)
		}

		private void ApplicationStopping()
		{
			//when gracefull shutdown is triggered/starting
		}

		private void ApplicationStopped()
		{
			//when gracefull shutdown has completed	
		}


		public void StopApplication()
		{
			//request full end of application
			_hostApplicationLifetime.StopApplication();
		}


		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					_logger.LogInformation("Worker1 running at: {time}", DateTimeOffset.Now);
					await Task.Delay(1000, stoppingToken);
				}
				Console.WriteLine("Exit main worker loop");
			}
			//the CancellationToken used as the ExecuteAsync stopper must use the ThrowIfCancellationRequested() method, so no other choice
			catch (OperationCanceledException exc)
			{
				Console.Error.WriteLine("Stopped (Operation cancelled)");

				//just to simulate a longer task ending 
				await Task.Delay(1500); 
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine("Unhandled exception occured");
				//TOOD: shuting down all services? Complete process in fact
			}
		}



		//Think we should not use that, apart for logging stuff
		public override Task StartAsync(CancellationToken cancellationToken)
		{
			Console.WriteLine("Starting worker");
			//avoid blocking/long code in here
			return base.StartAsync(cancellationToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			Console.WriteLine("Stoping worker");

			var swatch = Stopwatch.StartNew();
			await base.StopAsync(cancellationToken);

			Console.WriteLine($"Stoped worker 1 within {swatch.Elapsed.TotalMilliseconds}");
		}


	}
}
