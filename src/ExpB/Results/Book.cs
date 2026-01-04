using System;

namespace AgentAuthor.ExpB.Results
{
    public class Book
    {
        public string Title {get; set;}
        public string Description {get; set;}
        public Chapter[] Chapters {get; set;}

        public Book()
        {
            Title = string.Empty;
            Description = string.Empty;
            Chapters = new Chapter[]{};
        }
    }
}