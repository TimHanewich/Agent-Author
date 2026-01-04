using System;
using System.Collections.Generic;
using TimHanewich.Foundry;
using TimHanewich.Foundry.OpenAI.Responses;
using AgentAuthor.ExpB.Results;
using Newtonsoft.Json.Linq;

namespace AgentAuthor.ExpB.Agents
{
    public class Contributor
    {
        private FoundryResource Foundry {get; set;}
        private string ModelName {get; set;}
        
        public Contributor(FoundryResource fr, string model_name)
        {
            Foundry = fr;
            ModelName = model_name;
        }
    }
}