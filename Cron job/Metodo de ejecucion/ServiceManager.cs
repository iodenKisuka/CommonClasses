using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommonClass.MetododeEjecucion
{
    public class ServiceManager : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ProcessAutomatic _process;
        private readonly ILogger<ServiceManager> _log;
        public ServiceManager(IConfiguration config, ILogger<ServiceManager> logg, ProcessAutomatic process)
        {
            _config = config;
            _process = process;
            _log = logg;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();
                await scheduler.Start();
                JobManager.JobManagerVariable(_caso);

                var job = JobBuilder.Create<JobManager>()
                 .Build();

                var trigger = TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule(_config["horarioDeEjecucion"])
                .Build();

                await scheduler.ScheduleJob(job, trigger);

                //linea 45 de la clase serviceManager... descomentala si es que necesitas probar poniendo play sin necesidad de esperar el job
                await _process.Ejecution();

            }
            catch (Exception ex)
            {
                _log.LogError("ServiceManager - {ex.Message}", ex.Message);
            }
        }

    }
}
