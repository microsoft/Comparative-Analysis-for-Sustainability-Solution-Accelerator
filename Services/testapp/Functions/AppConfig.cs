// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

namespace Tester.ConsoleApp.Functions
{
    public class AppConfig
    {
        public string JobOwner { get; set; }
        public string Type { get; set; }
        public string disclosureNumber { get; set; }
        public string disclosureName { get; set; }
        public string disclosureRequirement { get; set; }
        public string disclosureRequirementDetail { get; set; }
        public string disclosureAnnex { get; set; }

        // Constructor to initialize properties
        public AppConfig(string myJobOwner, string myType, string myDisclosureNumber, string myDisclosureName, string myDisclosureRequirement, string myDisclosureRequirementDetail, string myDisclosureAnnex)
        {
            JobOwner = myJobOwner;
            Type = myType;
            disclosureNumber = myDisclosureNumber;
            disclosureName = myDisclosureName;
            disclosureRequirement = myDisclosureRequirement;
            disclosureRequirementDetail = myDisclosureRequirementDetail;
            disclosureAnnex = myDisclosureAnnex;
        }

        // Parameterless constructor for deserialization
        public AppConfig()
        {
            JobOwner = string.Empty;
            Type = string.Empty;
            disclosureNumber = string.Empty;
            disclosureName = string.Empty;
            disclosureRequirement = string.Empty;
            disclosureRequirementDetail = string.Empty;
            disclosureAnnex = string.Empty;
        }
    }
}
