using Quartz;
using System.Threading.Tasks;

namespace CommonClass.MetododeEjecucion
{
    public class JobManager : IJob
    {
        private static ProcessAutomatic _process;

        public static void JobManagerVariable(ProcessAutoatic process)
        {
            _process = process;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _ = _process.Ejecution();
            return Task.FromResult(true);
        }

    }
}