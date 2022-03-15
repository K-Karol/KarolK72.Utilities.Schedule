using System;

namespace KarolK72.Utilities.Schedule;

public class JobAttribute : Attribute
{
    private string _jobGuid;
    private string _jobName;
    private string _jobDescription;
    private Type _jobConfigurationType;
    public string JobGuid { get => _jobGuid; }
    public string JobName { get => _jobName; }
    public string JobDesciption { get => _jobDescription; }
    public Type JobConfigurationType { get => _jobConfigurationType; }
    public JobAttribute(string jobGuid, string jobName, Type jobConfigurationType, string jobDescription = null)
    {
        _jobGuid = jobGuid;
        _jobName = jobName;
        _jobConfigurationType = jobConfigurationType;
        _jobDescription = jobDescription;
    }
}
