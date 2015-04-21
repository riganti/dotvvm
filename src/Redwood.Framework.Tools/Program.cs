using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Redwood.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: Redwood.Framework.Tools.exe COMMAND");
                Console.WriteLine("Commands:");
                Console.WriteLine("  generateConfigSchema outputFile.json");
                return;
            }

            if (args[0] == "generateConfigSchema")
            {
                GenerateConfigSchema(args[1]);
            }
            else
            {
                Console.WriteLine("Invalid command!");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Generate JSON schema for redwood.json file.
        /// </summary>
        private static void GenerateConfigSchema(string outputFile)
        {
            var generator = new JsonSchemaGenerator();
            var schema = generator.Generate(typeof(RedwoodConfiguration));
            using (var textWriter = new StreamWriter(outputFile))
            {
                using (var writer = new JsonTextWriter(textWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    schema.WriteTo(writer);
                }
            }
        }
    }
}
