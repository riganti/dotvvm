using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ResourceManagement.ClientGlobalize
{
    public class JQueryGlobalizeRegisterTemplate
    {

	public JObject CultureInfoJson { get; set; }
	public string Name { get; set; }

        private System.Text.StringBuilder __sb;

        private void Write(string text) {
            __sb.Append(text);
        }

        private void WriteLine(string text) {
            __sb.AppendLine(text);
        }

        public string TransformText()
        {
            __sb = new System.Text.StringBuilder();
__sb.Append(@"



(function( window, undefined ) {

var Globalize;

if ( typeof require !== ""undefined""
	&& typeof exports !== ""undefined""
	&& typeof module !== ""undefined"" ) {
	// Assume CommonJS
	Globalize = require( ""globalize"" );
} else {
	// Global variable
	Globalize = window.dotvvm_Globalize;
}
Globalize.addCultureInfo(""");
__sb.Append( Name );
__sb.Append(@""", ""default"", ");
__sb.Append( CultureInfoJson.ToString() );
__sb.Append(@");

}( this ));


");

            return __sb.ToString();
        }
    }
}
