using System;

namespace AgentAuthor
{
    public class AuthorNodeOutput
    {
        public string[] subtasks {get; set;}
        public string? writing {get; set;}

        public AuthorNodeOutput()
        {
            subtasks = new string[]{};
            writing = null;
        }
    }
}