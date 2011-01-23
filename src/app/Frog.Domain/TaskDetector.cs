using System.Collections.Generic;
using Frog.Domain.Specs;

namespace Frog.Domain
{
    public interface TaskDetector
    {
        IList<ExecTask> Detect();
    }
}