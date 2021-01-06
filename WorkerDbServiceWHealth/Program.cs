using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.ServiceProcess;

namespace WorkerDbServiceWHealth
{
	public class Program
	{
		//Widely taken from/inspired by https://app.pluralsight.com/library/courses/building-aspnet-core-hosted-services-net-core-worker-services/
		//and also its equivalence in MSFT docs: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-5.0

		//TODO: looking for a way to auto-install the current service as in TopShelf (see: https://github.com/Topshelf/Topshelf/blob/develop/src/Topshelf/Runtime/DotNetCore/DotNetCoreHostEnvironment.cs#L159)

		public static void Main(string[] args)
		{
			//for auto installing? No, can just start/stop etc... no installation possible...
			//ServiceController ctrl = new ServiceController()

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

				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
