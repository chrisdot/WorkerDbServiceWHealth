using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ServiceProcess;

namespace testWorkerService
{
	public class Program
	{
		//https://app.pluralsight.com/library/courses/building-aspnet-core-hosted-services-net-core-worker-services/
		//using System.ServiceProcess
		public static void Main(string[] args)
		{
			//ServiceController ctrl = new ServiceController()
			//for auto installing

			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				//Windows service declaration that will (only if it runs in windows service mode)):
				//- configures the host to use a WindowsServiceLifetime
				//- sets the ContentRootPath to AppContext.baseDirectory
				//- Enables logging to the Windows event log
				.UseWindowsService() //using the extension for windows services (windows event logging, registration, etc...)
				.ConfigureServices((hostContext, services) =>
				{
					services.PostConfigure<HostOptions>(option => {
						//max timeout for a hosted service for shutdown before being forced to shutdown
						option.ShutdownTimeout = TimeSpan.FromSeconds(10); 
					});

					//Will start in this order: first Worker1(), then Worker2()
					//each hosted service is started sequentially (waiting for Worker1 to be completely started before starting Worker2)
					services.AddHostedService<Worker1>();
					services.AddHostedService<Worker2>();
					//BUT: When gracefully stopped, will be stopped in the other way round (Worker2 stopped before Worker1)

				});
	}
}
