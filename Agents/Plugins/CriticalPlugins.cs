using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents.Plugins
{
    internal sealed class CriticalPlugins
    {
        internal CriticalPlugins() { }

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
