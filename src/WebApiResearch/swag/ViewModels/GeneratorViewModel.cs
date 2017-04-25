using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.TypeScript;
using NSwag.SwaggerGeneration.WebApi;

namespace swag.ViewModels
{
	public class GeneratorViewModel : DotvvmViewModelBase
	{

	    public string CSharpPath { get; set; } = "d:\\temp\\SwaggerTest\\ApiClient.cs";

	    public string TSPath { get; set; } = "d:\\temp\\SwaggerTest\\ApiClient.ts";
        

	    public async Task GenerateCSharp()
	    {
	        var document = await GetSwaggerDocument();

	        var settings = new SwaggerToCSharpClientGeneratorSettings()
	        {
                GenerateSyncMethods = true
            };

	        var generator = new SwaggerToCSharpClientGenerator(document, settings);
            Context.ReturnFile(Encoding.UTF8.GetBytes(generator.GenerateFile()), "ApiClient.cs", "text/plain");
	        //File.WriteAllText(CSharpPath, generator.GenerateFile());
	    }

	    public async Task GenerateTS()
	    {
	        var document = await GetSwaggerDocument();

	        var settings = new SwaggerToTypeScriptClientGeneratorSettings()
	        {
                Template = TypeScriptTemplate.Fetch
	        };

	        var generator = new SwaggerToTypeScriptClientGenerator(document, settings);
            Context.ReturnFile(Encoding.UTF8.GetBytes(generator.GenerateFile()), "ApiClient.ts", "text/plain");
            //File.WriteAllText(TSPath, generator.GenerateFile());
        }



	    private async Task<SwaggerDocument> GetSwaggerDocument()
	    {
	        var settings = new WebApiToSwaggerGeneratorSettings();
	        var generator = new WebApiToSwaggerGenerator(settings);

	        var controllers = typeof(GeneratorViewModel)
	            .GetTypeInfo()
	            .Assembly.GetTypes()
	            .Where(t => typeof(Controller).IsAssignableFrom(t));
	        return await generator.GenerateForControllersAsync(controllers);
	    }
	}
}

