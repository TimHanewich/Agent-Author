using System;

namespace AgentAuthor.ExpB.Results
{
    public class Chapter
    {
        public string Title {get; set;}
        public string Purpose {get; set;} // purpose of this chapter, what it will contain, etc.
        public Section[] Sections {get; set;}
        
        public Chapter()
        {
            Title = string.Empty;
            Purpose = string.Empty;
            Sections = new Section[]{};
        }
    }
}