using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDbServiceWHealth
{
	internal class MyHealthCheckPublisher : IHealthCheckPublisher
	{
		public MyHealthCheckPublisher()
		{
		}

		public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			if(report.Status == HealthStatus.Healthy)
			{
				//nothing to do, everything's alright
			}
			else
			{
				//we have a problem, we might have to stop services
			}
			return Task.CompletedTask;
		}
	}
}