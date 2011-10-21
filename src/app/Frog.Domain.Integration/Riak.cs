using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CorrugatedIron;

namespace Frog.Domain.Integration
{
    public class Riak
    {
        public static  IRiakCluster GetConnectionManager(string host, int port)
        {
            if (cluster == null)
            {
                var tempFileName = Path.GetTempFileName();
                File.AppendAllText(tempFileName, String.Format(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <configSections>
    <section name=""riakConfig"" type=""CorrugatedIron.Config.RiakClusterConfiguration, CorrugatedIron""/>
  </configSections>
  <riakConfig nodePollTime=""5000"" defaultRetryWaitTime=""200"" defaultRetryCount=""3"">
    <nodes>
      <node name=""dev1"" hostAddress=""{0}"" pbcPort=""{1}"" restScheme=""http"" restPort=""8098"" poolSize=""20"" />
    </nodes>
  </riakConfig>
</configuration>
", host, port));
                cluster = RiakCluster.FromConfig("riakConfig", tempFileName);
            }
            return cluster;
        }

        private static IRiakCluster cluster;

        public static string KeyGenerator(string repoUrl)
        {
            return String.Concat(
                new MD5CryptoServiceProvider().ComputeHash(
                    Encoding.UTF8.GetBytes(repoUrl)).Select(b => b.ToString("x2")));
        }
    }
}
