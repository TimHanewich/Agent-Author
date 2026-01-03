using System;
using Newtonsoft.Json;
using TimHanewich.Foundry;
using TimHanewich.Foundry.OpenAI.Responses;

namespace AgentAuthor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FoundryResource fr = new FoundryResource("https://xxx.services.ai.azure.com");
            fr.ApiKey = "6ElIJZ2j...";

            AuthorNode author = new AuthorNode(fr, "gpt-5.2", "Write a book about psychology.");
            author.WorkAsync().Wait();

            while (true)
            {
                Console.WriteLine("---------------");
                Console.WriteLine(JsonConvert.SerializeObject(author, Formatting.Indented));
                Console.WriteLine();
                Console.Write("Which subtask do you want to do next? ");
                string? input = Console.ReadLine();
                int selection = 0;
                if (input != null)
                {
                    selection = Convert.ToInt32(input);
                }

                if (author.SubAuthors == null)
                {
                    Console.WriteLine("not worked on yet.");
                    return;
                }
                else if (author.SubAuthors.Length == 0)
                {
                    Console.WriteLine("No assigned subauthors.");
                    return;
                }

                //Flesh out
                author = author.SubAuthors[0];
                author.WorkAsync().Wait();
            }


        }
    }
}