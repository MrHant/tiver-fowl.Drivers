using System;
using System.Collections.Generic;
using System.Linq;

namespace Tiver.Fowl.Drivers.Downloaders
{
    public static class Extensions
    {
        public static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException("ex");
            }

            var innerException = ex.InnerException;
            do
            {
                yield return innerException;
                innerException = innerException.InnerException;
            } while (innerException != null);
        }

        public static string GetInnerExceptionMessages(this Exception ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }

            return string.Join(" ", ex.GetInnerExceptions().Select(e => e.Message));
        }

        public static string GetAllExceptionsMessages(this Exception ex)
        {
            return ex.Message + " " + ex.GetInnerExceptionMessages();
        }
    }
}