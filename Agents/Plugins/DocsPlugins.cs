using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using Agents.Entities;
using System.ComponentModel;


namespace Agents.Plugins
{
    internal sealed class DocsPlugins
    {
        internal DocsPlugins() { }

        [KernelFunction("GetDocument")]
        [Description("Get the content of a document")]
        public string GetDocument([Description("Document label")] string label)
        {
            if (label.ToLower().Contains("a")) return "Sono nato il 18 Aprile";
            if (label.ToLower().Contains("b")) return "Sono nato il 18 Maggio";

            return "Doumento non trovato!";
           
        }

    }
}
