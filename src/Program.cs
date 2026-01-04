using System;
using System.Net;
using Newtonsoft.Json;
using TimHanewich.Foundry;
using TimHanewich.Foundry.OpenAI.Responses;
using AgentAuthor.ExpA;

namespace AgentAuthor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FoundryResource fr = new FoundryResource("https://xxx.services.ai.azure.com");
            fr.ApiKey = "6ElIJZ2j...";

            AuthorNode author = new AuthorNode(fr, "gpt-5.2", "Write a childrens book about dogs that go to heaven with 5 chapters. Delegate each chapter to another contributor.");
            
            author.WorkAsync().Wait();
            Console.WriteLine(JsonConvert.SerializeObject(author, Formatting.Indented));


            //Recursive
            Console.WriteLine("HERE WE GO!");
            RecursiveWorkAsync(author).Wait();
            System.IO.File.WriteAllText(@"C:\Users\timh\Downloads\Agent-Author\result.json", JsonConvert.SerializeObject(author, Formatting.Indented));
            
        }
    
        //Returns the number of authors created
        public static async Task<int> RecursiveWorkAsync(AuthorNode author, int depth = 0)
        {
            int ToReturn = 0;

            //Determine tab depth
            string Tabs = new string('\t', depth);
            
            //Run it
            Console.Write(Tabs + "Running author with prompt of " + author.AssignedTask.Length.ToString("#,##0") + " charaters... ");
            await author.WorkAsync();

            //Print the results
            if (author.writing != null && author.SubAuthors != null && author.SubAuthors.Length > 0)
            {
                Console.WriteLine("It decided to write " + author.writing.Length.ToString("#,##0") + " characters AND delegate to " + author.SubAuthors.Length.ToString() + " authors.");
            }
            else if (author.writing != null)
            {
                Console.WriteLine("it wrote " + author.writing.Length.ToString("#,##0") + " characters!");
            }
            else if (author.SubAuthors != null)
            {
                Console.WriteLine("it delegated to " + author.SubAuthors.Length.ToString("#,##0") + " authors!");
            }
            else
            {
                Console.WriteLine("NO DECISION MADE! THIS IS A PROBLEM!");
            }

            //Recursively go through each child
            if (author.SubAuthors != null)
            {
                ToReturn = ToReturn + author.SubAuthors.Length; //Add the IMEDIATE number of authors made by the parent author provided
                foreach (AuthorNode sub in author.SubAuthors)
                {
                    int AuthorsMadeByThisSub = await RecursiveWorkAsync(sub, depth + 1); //And then also factor in the count of the subs each sub of this parent author made
                    ToReturn = ToReturn + AuthorsMadeByThisSub;
                }
            }
            
            //Return the total # of authors made (and recursively)
            return ToReturn;
        }
    }
}