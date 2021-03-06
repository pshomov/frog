﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace SaaS.Engine
{
    [DataContract(Namespace = "Runz")]
    public sealed class BuildId : AbstractIdentity<Guid>
    {
        public const string TagValue = "build";

        public BuildId(Guid id)
        {
            Id = id;
        }

        public override string GetTag()
        {
            return TagValue;
        }


        [DataMember(Order = 1)]
        public override Guid Id { get; protected set; }

        public BuildId()
        {
            Id = Guid.NewGuid();
        }
    }

    [DataContract(Namespace = "Runz")]
    public sealed class ProjectId : AbstractIdentity<string>
    {
        public const string TagValue = "project";

        public ProjectId(string id)
        {
            id = Regex.Replace(id, @"//.+:.+@", "//", RegexOptions.IgnoreCase);
            Id = id.Replace('/', '_').Replace(':','_');
        }

        public override string GetTag()
        {
            return TagValue;
        }


        [DataMember(Order = 1)]
        public override string Id { get; protected set; }

    }
    [DataContract(Namespace = "Runz")]
    public sealed class TerminalId : AbstractIdentity<Guid>
    {
        public const string TagValue = "terminal";

        public TerminalId(Guid id)
        {
            Id = id;
        }

        public override string GetTag()
        {
            return TagValue;
        }


        [DataMember(Order = 1)]
        public override Guid Id { get; protected set; }

        public TerminalId()
        {
            Id = Guid.NewGuid();
        }
    }

    [DataContract(Namespace = "Runz")]
    public sealed class AgentId : AbstractIdentity<Guid>
    {
        public const string TagValue = "agent";

        public AgentId(Guid id)
        {
            Id = id;
        }

        public override string GetTag()
        {
            return TagValue;
        }


        [DataMember(Order = 1)]
        public override Guid Id { get; protected set; }

        public AgentId()
        {
            Id = Guid.NewGuid();
        }
    }
}
