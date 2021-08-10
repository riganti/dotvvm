using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.PerfTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //ParserTest();
            var t = new SerializerTests(false);
            t.Serialize();
            Console.WriteLine($"VM size: {t.LastViewModel.Length/1024.0/1024.0}MB");

            //TestForever(() => {
            //    //var jobj = JObject.Parse(t.LastViewModel);
            //    //var jobj = JObject.Load(t.request.ViewModelJson.CreateReader());
            //});


            TestForever(t.Deserialize);
        }

        private static void ParserTest()
        {
            var t = new ParserTests();
            t.DownloadData("https://news.ycombinator.com/");
            TestForever(t.TokenizeAndParse);
        }

        public static void TestForever(Action testMethod, int repeat = 1)
        {
            Console.WriteLine($"testing {testMethod.Method.DeclaringType.Name}.{testMethod.Method.Name}, {repeat} times per measurement");
            Console.WriteLine();
            var sw = new Stopwatch();
            List<TimeSpan> m = new List<TimeSpan>();
            int mi = 0;
            while (true) {
                sw.Start();
                for (int i = 0; i < repeat; i++) {
                    testMethod();
                }
                sw.Stop();
                m.Add(sw.Elapsed);
                Console.WriteLine($"#{mi:000}: {sw.Elapsed}                                                                        ");
                Console.Write($"min: {m.Min()} \tmax: {m.Max()} \tavg: {TimeSpan.FromTicks((long)m.Average(t => t.Ticks))}\r");
                sw.Reset();
                mi++;
            }
        }
    }
}
