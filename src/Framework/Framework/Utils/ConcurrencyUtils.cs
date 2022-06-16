using System;
using System.Collections.Immutable;
using System.Threading;

namespace DotVVM.Framework.Utils
{
    static class ConcurrencyUtils
	{
		public static T CasChange<T>(ref T xRef, Func<T, T> f)
			where T : class
		{
		Retry:
			var x1 = xRef;
			var x2 = f(x1);

			var oldValue = Interlocked.CompareExchange(ref xRef, x2, x1);
			if (oldValue != x1)
			{
				// unsuccessfully write
				goto Retry;
			}

			return x2;
		}
	}
}
