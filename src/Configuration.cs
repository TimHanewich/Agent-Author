using System;
using TimHanewich.Foundry;

namespace AgentAuthor
{
    public class Configuration
    {
        public FoundryResource Foundry {get; set;}
        public string ModelName {get; set;}

        public Configuration()
        {
            Foundry = new FoundryResource("");
            ModelName = "";
        }

        public static Configuration LoadDefault()
        {
            Configuration ToReturn = new Configuration();
            ToReturn.Foundry = new FoundryResource("(endpoint here)");
            ToReturn.Foundry.ApiKey = "(api key here)";
            ToReturn.ModelName = "(model name here)";
            return ToReturn;
        }
    }
}