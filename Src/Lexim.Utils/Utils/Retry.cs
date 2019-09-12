using System;
using System.Collections.Generic;
using System.Threading;

namespace Lexim.Utils
{
#if LEXIM_UTILS
    public static class Retry
#else
    internal static class Retry
#endif

    {
        public class AttemptException: Exception
        {
            public int Attempt { get; }

            public AttemptException(Exception inner, int attempt): base($"An exception was thrown in attempt {attempt}", inner)
            {
                Attempt = attempt;
            }
        }

        public class RetryException : Exception
        {
            public int Retries { get; }

            public List<AttemptException> AttemptExceptions { get; }

            public RetryException(int retries, List<AttemptException> attemptExceptions) : base(
                "Multiple retries for the same action failed. See the AttemptExceptions property for details.")
            {
                AttemptExceptions = attemptExceptions;
                Retries = retries;
            }
        }

        public static void Do(Action action, int retries, Action<Exception> onException = null)
        {
            RetryException ex = null;
            var attempt = 1;
            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception e)
                {
                    if (ex == null)
                        ex = new RetryException(retries, new List<AttemptException>());

                    var attemptException = new AttemptException(e, attempt);

                    onException?.Invoke(attemptException);

                    ex.AttemptExceptions.Add(attemptException);

                    attempt++;
                    retries--;

                    if (retries <= 0)
                        throw ex;

                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }

        public static T Do<T>(Func<T> action, int retries, Action<Exception> onException = null)
        {
            RetryException ex = null;
            var attempt = 1;
            while (true)
            {
                try
                {
                   return action();
                }
                catch (Exception e)
                {
                    if (ex == null)
                        ex = new RetryException(retries, new List<AttemptException>());

                    var attemptException = new AttemptException(e, attempt);

                    onException?.Invoke(attemptException);

                    ex.AttemptExceptions.Add(attemptException);

                    attempt++;
                    retries--;

                    if (retries <= 0)
                        throw ex;

                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}
