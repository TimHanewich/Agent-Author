using System;

namespace AgentAuthor.ExpB.Results
{
    public class Section
    {
        public string Heading {get; set;} //the title
        public string Goal {get; set;} //the stated goal of this section, as planned by the contributor
        public string Content {get; set;}
        
        public Section()
        {
            Heading = string.Empty;
            Goal = string.Empty;
            Content = string.Empty;
        }
    }
}