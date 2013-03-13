using System.Collections.Generic;
using System.Linq;

namespace Frog.Domain
{
    public class PipelineStatus
    {
        public List<TaskInfo> Tasks = new List<TaskInfo>();
        public override string ToString()
        {
            return string.Format("Pipeline:\r\n{0}", Tasks.Select(info => info.ToString()).Aggregate((info, taskInfo) => info + "\r\n"+taskInfo ));
        }
    }
}