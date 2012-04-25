using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Frog.Specs.Support
{
    public static class AssertionHelpers
    {
        public static T WithRetries<T>(Func<T> query, int retries = 100)
        {
            while (true)
            {
                try
                {
                    return query();
                }
                catch (NoSuchElementException)
                {
                    if (retries > 0)
                    {
                        retries = SleepABit(retries);
                    }
                    else throw;
                }
            }
        }

        public static void WithRetries(Action action, int retries = 100)
        {
            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (AssertionException)
                {
                    if (retries > 0)
                    {
                        retries = SleepABit(retries);
                    }
                    else throw;
                }
                catch (NoSuchElementException)
                {
                    if (retries > 0)
                    {
                        retries = SleepABit(retries);
                    }
                    else throw;
                }
            }
        }

        static int SleepABit(int retries)
        {
            retries--;
            Thread.Sleep(300);
            return retries;
        }
    }
}