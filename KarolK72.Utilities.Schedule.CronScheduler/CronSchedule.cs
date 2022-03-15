using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KarolK72.Utilities.Schedule.CronScheduler
{
    [Scheduler("CronScheduler")]
    public class CronJobSchedule<TJob> : IJobSchedule where TJob : IJob
    {
        #region Private Fields
        private ILogger<CronJobSchedule<TJob>> _logger;
        private DateTime? _nextExecutionTime = null;
        private DateTime? _startOfExecution = null;
        private DateTime? _endOfExecution = null;
        private string _cronExpressionString = string.Empty;
        private NCrontab.CrontabSchedule _cronTabSchedule = null;
        private Guid _jobGuid = Guid.Empty;
        private string _jobName = string.Empty;
        private string _jobInstanceID = string.Empty;
        private string _jobGroupID = string.Empty;
        private Type _jobConfigurationType = null;
        private object _jobConfigurationObject = null;
        private IServiceProvider _serviceProvider = null;
        #endregion

        #region Public Fields
        #endregion

        #region Interface Properties
        public DateTime? NextExecutionTime => _nextExecutionTime;
        public Guid JobGuid => _jobGuid;
        public string JobName => _jobName;
        public string JobInstanceID => _jobInstanceID;
        public string JobGroupID => _jobGroupID;
        public Type JobConfigurationType => _jobConfigurationType;

        //public JobScheduleInfo JobScheduleInfo => new JobScheduleInfo()
        //{
        //    JobGuid = _jobGuid,
        //    JobConfigurationType = _jobConfigurationType,
        //    JobGroupID = _jobGroupID,
        //    JobInstanceID = _jobInstanceID,
        //    JobName = _jobName,
        //    NextExecutionTime = _nextExecutionTime,
        //};
        #endregion

        #region Interface Methods
        public void CalculateNextExecutionTime()
        {
            _nextExecutionTime = _cronTabSchedule.GetNextOccurrence(DateTime.Now);
        }

        public IJob CreateInstance()
        {
            //Do some form of dependency injection?
            if (_serviceProvider == null)
            {
                IJob tmp = (IJob)Activator.CreateInstance(typeof(TJob));
                tmp.ConfigurationObject = _jobConfigurationObject;
                return tmp;
            }


            //_serviceProvider.
            //TypeInfo typeInfo = typeof(TJob).GetTypeInfo();

            //var constructor = typeInfo.DeclaredConstructors.Where(c => !c.IsStatic).OrderByDescending(dc => dc.GetParameters().Count()).FirstOrDefault();
            //if(constructor == null)
            //{
            //    _logger.LogWarning("No contructor found");
            //    IJob tmp = (IJob)Activator.CreateInstance(typeof(TJob));
            //    tmp.ConfigurationObject = _jobConfigurationObject;
            //    return tmp;
            //}

            //var paramaters = constructor.GetParameters();
            //var paras = new object[paramaters.Length];
            //for(int p = 0; p < paramaters.Length; p++)
            //{
            //    paras[p] =  _serviceProvider.GetService(paramaters[0].ParameterType);
            //}

            //IJob obj = (IJob)constructor.Invoke(paras);
            //obj.ConfigurationObject = _jobConfigurationObject;

            IJob obj = (IJob)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(TJob));
            obj.ConfigurationObject = _jobConfigurationObject;

            return obj;
        }
        #endregion

        public CronJobSchedule(CronJobSchedulerConfiguration configuration, ILogger<CronJobSchedule<TJob>> logger, IServiceProvider serviceProvider = null, object jobConfigurationObject = null)
        {
            var jobAttr = (JobAttribute)typeof(TJob).GetCustomAttribute(typeof(JobAttribute), false);

            if (jobAttr == null)
            {
                throw new ArgumentException($"{typeof(TJob).FullName} does not contain the {typeof(JobAttribute).FullName} attribute");
            }

            _logger = logger;
            _startOfExecution = configuration.StartOfExecution;
            _endOfExecution = configuration.EndOfExecution;
            _jobInstanceID = configuration.JobInstanceID;
            _jobGroupID = configuration.JobGroupID;
            _cronExpressionString = configuration.CronExpression;
            _cronTabSchedule = NCrontab.CrontabSchedule.Parse(_cronExpressionString);
            _jobConfigurationObject = jobConfigurationObject;
            _serviceProvider = serviceProvider;
            _jobConfigurationType = jobAttr.JobConfigurationType;
            _jobGuid = Guid.Parse(jobAttr.JobGuid);
            _jobName = jobAttr.JobName;


        }
    }
}
