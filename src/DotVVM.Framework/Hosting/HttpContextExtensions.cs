using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting
{
    public static class HttpContextExtensions
    {
		public static void Write(this HttpResponse response, string text)
		{
			using (var writer = new StreamWriter(response.Body, Encoding.UTF8, 4096, leaveOpen: true))
			{
				writer.Write(text);
			}
		}
    }
}
