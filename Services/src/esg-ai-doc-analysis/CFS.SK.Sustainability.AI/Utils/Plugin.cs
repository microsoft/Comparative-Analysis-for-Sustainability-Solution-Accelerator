// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFS.SK.Sustainability.AI.Utils
{
    internal class Plugin
    {
        public static string GetPluginDirectoryPath(string plugInRoot, 
                                                string plugInCategory, 
                                                string? path = null)
        {
            if (path == null)
                path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            return Path.Combine(
                path, 
                plugInRoot,
                plugInCategory);
        }

        public static bool IsRegistered(Kernel Kernel, string PluginName)
        {
            return (Kernel.Plugins.Where(x => x.Name == PluginName).SingleOrDefault() != null);
        }
    }
}
