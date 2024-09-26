// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFS.SK.Sustainability.AI.Utils
{
    public static class DictionaryExtension
    {
        //Extenstion Method for Dictionary<string,object>
        public static string GetValue(this Dictionary<string, object> dict, string KeyName) {
            //check parameters
            if (dict == null) throw new ArgumentNullException();

            if (dict.ContainsKey(KeyName))
            {
                return dict[KeyName].ToString();
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

    }
}
