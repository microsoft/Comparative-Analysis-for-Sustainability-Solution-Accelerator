// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Tester.ConsoleApp.Functions;

namespace Tester.ConsoleApp.Helpers
{
    public static class ConfigHelper
    {
        public static AppConfig GetAppConfig(string standardType, IConfiguration config)
        {
            var appConfig = new AppConfig();

            if (standardType.Equals("CSRD", StringComparison.OrdinalIgnoreCase))
            {
                appConfig.JobOwner = config["CSRD:JobOwner"] ?? string.Empty;
                appConfig.Type = config["CSRD:Type"] ?? string.Empty;
                appConfig.disclosureName = config["CSRD:disclosureName"] ?? string.Empty;
                appConfig.disclosureNumber = config["CSRD:disclosureNumber"] ?? string.Empty;
                appConfig.disclosureRequirement = config["CSRD:disclosureRequirement"] ?? string.Empty;
                appConfig.disclosureRequirementDetail = config["CSRD:disclosureRequirementDetail"] ?? string.Empty;
            }
            else if (standardType.Equals("GRI", StringComparison.OrdinalIgnoreCase))
            {
                appConfig.JobOwner = config["GRI:JobOwner"] ?? string.Empty;
                appConfig.Type = config["GRI:Type"] ?? string.Empty;
                appConfig.disclosureName = config["GRI:disclosureName"] ?? string.Empty;
                appConfig.disclosureNumber = config["GRI:disclosureNumber"] ?? string.Empty;
                appConfig.disclosureRequirement = config["GRI:disclosureRequirement"] ?? string.Empty;
                appConfig.disclosureRequirementDetail = config["GRI:disclosureRequirementDetail"] ?? string.Empty;
                appConfig.disclosureAnnex = config["GRI:disclosureAnnex"] ?? string.Empty;
            }
            else
            {
                throw new ArgumentException($"Invalid input {standardType}. Valid values: 'CSRD' or 'GRI'.");
            }

            return appConfig;
        }
    }
}
