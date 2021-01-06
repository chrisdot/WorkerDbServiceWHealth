using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using HealthChecks.UI.Client;

namespace WorkerDbServiceWHealth
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			//sql server connection string to be tested
			string connectionString = Configuration["WorkerDbServiceWHealth:ConnectionString"];

			services.AddControllers();

			//declaring healthcheck usage
			services.AddHealthChecks()
				.AddProcessAllocatedMemoryHealthCheck(100, name: "max memory")
				.AddSqlServer(connectionString, name: "database", failureStatus: HealthStatus.Unhealthy, timeout: TimeSpan.FromSeconds(10))
				//TODO: see also filtering by tag


				//Example from HealthChecks.Uris nuget package to test dependant URLs services
				//.AddUrlGroup(...)

				//adding  a custom healthcheck, based on a IHealthCheck interface
				//.AddCheck(...)

				;

			services.Configure<HealthCheckPublisherOptions>(opts => {
				//inital delay before doing first healthCheck test
				opts.Delay = TimeSpan.FromSeconds(5);
				//then make the test every 10s
				opts.Period = TimeSpan.FromSeconds(10);
				//add a predicate if we want to filter eg by tag)
				//opts.Predicate = 
				//timeout for the duration of the health check: if it does timeout it will report a unhealthy status
				opts.Timeout = TimeSpan.FromSeconds(20);
			});

			services.AddSingleton<IHealthCheckPublisher, MyHealthCheckPublisher>();

			//or adding it via lambda
			//.AddCheck("SQl check", () =>
			//{
			//	using (var connection = new SqlConnection(connectionString))
			//	{
			//		try
			//		{
			//			connection.Open();
			//			return HealthCheckResult.Healthy();
			//		}
			//		catch
			//		{
			//			return HealthCheckResult.Unhealthy();
			//		}
			//	}
			//});

			//this is using a additional nuget package (AspNetCore.HealthChecks.UI)
			services.AddHealthChecksUI(setup=>{
				//every 10 sec
				setup.SetEvaluationTimeInSeconds(10);
				//allow max 1 concurrent connection
				setup.SetApiMaxActiveRequests(1);
				
				// Set the maximum history entries by endpoint that will be served by the UI api middleware
				setup.MaximumHistoryEntriesPerEndpoint(50);

				setup.AddHealthCheckEndpoint("CurrentNodeHealth", "/healthforui");
			}).AddInMemoryStorage(); //TODO: see maximum size in memory? See for cleanup?
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/health", new HealthCheckOptions()
				{
					ResultStatusCodes =
					{
						//dictionnary mappings from healthStatuses to HTTP response codes
						[HealthStatus.Healthy] = StatusCodes.Status200OK,
						[HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
						[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
					},
					ResponseWriter = WriteHealthResponse
				});

				//could also restrain to a certain host (only available on a certain host, a kind of filtering)
				//endpoints.MapHealthChecks("/health").RequireHost("www.test.com:8000");


				endpoints.MapHealthChecks("/healthforui", new HealthCheckOptions()
				{
					Predicate = _ => true,
					ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
				});

				endpoints.MapHealthChecksUI(opt =>
				{
					opt.UIPath = "/healthui";
					opt.ApiPath = "/healthuiapi";
				});

			});
		}

		private Task WriteHealthResponse(HttpContext httpContext, HealthReport response)
		{
			httpContext.Response.ContentType = "application/json";

			var respJson = new JObject(
				new JProperty("Overall status", response.Status.ToString()),
				new JProperty("TotalCheckDuration", response.TotalDuration.TotalSeconds.ToString()),
				new JProperty("DependencyHealthChecks", new JObject(response.Entries.Select(dicItem =>
					new JProperty(dicItem.Key,
					new JObject(
						new JProperty("Status", dicItem.Value.Status.ToString()),
						new JProperty("Duration", dicItem.Value.Duration.TotalSeconds.ToString())
					))
				))));

			return httpContext.Response.WriteAsync(respJson.ToString(Formatting.Indented));
		}
	}

}
