using System;
using System.Collections.Generic;
using CorrugatedIron;
using CorrugatedIron.Exceptions;
using CorrugatedIron.Models;

namespace Frog.Domain.Integration
{
    public static class CorrugatedIronExts
    {
        public static T Get<T>(this IRiakClient client, string bucket, string key)
        {
            var result = client.Get(bucket, key);
            if (result.IsSuccess)
            {
                return result.Value.GetObject<T>();
            }
            if (!result.IsSuccess && result.ResultCode == ResultCode.NotFound)
            {
                throw new KeyNotFoundException(result.ErrorMessage);
            }
            throw new RiakException(result.ErrorMessage);
        }

        public static void Put(this IRiakClient client, string bucket, string key, object value)
        {
            var riakResult = client.Put(new RiakObject(bucket, key, value));
            if (riakResult.IsSuccess) return;
            throw new RiakException(riakResult.ErrorMessage);
        }

        public static IEnumerable<string> ListKeyz(this IRiakClient client, string bucket)
        {
            var riakResult = client.ListKeys(bucket);
            if (riakResult.IsSuccess) return riakResult.Value;
            throw new RiakException(riakResult.ErrorMessage);
        }
    }
    public class KeyNotFoundException : RiakException
    {
        public KeyNotFoundException(uint errorCode, string errorMessage)
            : base(errorCode, errorMessage)
        {
        }

        public KeyNotFoundException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}