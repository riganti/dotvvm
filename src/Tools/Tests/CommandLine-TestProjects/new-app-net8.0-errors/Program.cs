using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace dotvvm_test_app;
public class Program
{
    public static void Main(string[] args)
    {
        var host = WebHost.CreateDefaultBuilder(args)
            .UseIISIntegration()
            .UseStartup<Startup>()
            .Build();

        host.Run();
    }
}
