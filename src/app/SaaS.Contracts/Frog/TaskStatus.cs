using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaaS.Engine
{
    public enum TaskStatus
    {
        NotStarted,
        Started,
        FinishedSuccess,
        FinishedError
    }
    public enum BuildTotalEndStatus
    {
        Error,
        Success
    }

}
