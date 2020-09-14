using System;
using System.Collections.Generic;
using System.Text;

namespace MS.GTA.BOTService.Common.Configuration
{
    public class CosmosDBConfiguration
    {
        public string Uri { get; set; }

        public string Key { get; set; }

        public string Database { get; set; }

        public string GTACommonContainer { get; set; }

        public string GTABOTContainer { get; set; }

        

    }
}
