using System;
using TimHanewich.Foundry.OpenAI.Responses;
using Newtonsoft.Json.Linq;

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

        public async Task PlanBookAsync(string desired_book_info)
        {
            string prompt = @"You are the project lead for a new book that you and a team of your close contributors will author. 
            
Your role is to consider the book you have been assigned to write and determine the critical high level details about the book, such as its title, description, and what chapters it should contain.

Always answer in JSON in the following format for example:
{
    ""title"": ""Why the Future of High-Tech is High-Touch"",
    ""description"": ""In an era of hyper-automation and artificial intelligence, The Analog Pulse explores the surprising re-humanization of the global economy. As digital tools become commodities, the book argues that the most valuable assets of the next decade will be the things computers cannot replicate: physical presence, tactile experiences, and radical empathy. From the resurgence of vinyl records to the rise of slow-tech retreats, this book provides a roadmap for staying relevant in an increasingly automated world."",
    ""chapters"":
    [
        {
            ""title"": ""Chapter 1: The Digital Ceiling"",
            ""purpose"": ""Establish the problem by identifying the point of diminishing returns in our digital lives. It examines the psychological and economic fatigue caused by constant connectivity and argues that technology has reached a saturation point where more features no longer lead to more value. By defining the Digital Ceiling, the chapter sets the stage for why a shift toward the physical and human is not just a trend, but a necessity for future growth.""
        },
        {
            ""title"": ""Chapter 2: The Tactile Economy"",
            ""purpose"": ""Explore the rising market value of physical experiences and tangible goods. It shifts the focus from the cloud back to the earth, analyzing why consumers are increasingly drawn to artisanal crafts, print media, and analog tools. The goal is to prove that in an automated world, the imperfections of the physical world—texture, weight, and craftsmanship—become premium status symbols and vital components of modern brand loyalty.""
        },
        {
            ""title"": ""Chapter 3: Radical Empathy as a Service"",
            ""purpose"": ""Redefine Human Resources as a competitive strategy rather than a back-office function. It argues that as AI masters logic and data, the soft skills of high-level empathy, emotional intelligence, and complex conflict resolution become the ultimate high-value commodities. It provides a framework for how businesses can move beyond transactional User Experience to create deep, meaningful human connections that technology cannot replicate.""
        },
        {
            ""title"": ""Chapter 4: The Hybrid Future"",
            ""purpose"": ""This final chapter serves as a synthesis, offering a practical roadmap for integrating digital efficiency with analog depth. Its purpose is to move away from an either/or mentality, instead proposing a Centaur model where AI handles the speed while humans provide the soul. It concludes the book by providing actionable rituals and leadership strategies for maintaining balance, ensuring the reader leaves with a clear vision of how to thrive in a high-tech, high-touch world.""
        }
    ]
}

As you can see in the example above, your job is to determine the book's title, description, and then the chapters it will contain, each with its own title and purpose.
";

            //Load config
            Configuration config = Configuration.LoadDefault();

            //Draft request
            ResponseRequest rr = new ResponseRequest();
            rr.Model = config.ModelName;
            rr.Inputs.Add(new Message(Role.developer, prompt));
            rr.Inputs.Add(new Message(Role.user, desired_book_info));
            rr.RequestedFormat = ResponseFormat.JsonObject;
            
            //request
            Response r = await config.Foundry.CreateResponseAsync(rr);

            //Strip out the response txt
            string response = "";
            foreach (Exchange ex in r.Outputs)
            {
                if (ex is Message msg)
                {
                    if (msg.Text != null)
                    {
                        response = msg.Text;
                    }
                }
            }

            //If no response
            if (response == "")
            {
                throw new Exception("Unable to find book response.");
            }

            //Parse as Json
            JObject responseJO = JObject.Parse(response);

            //Title
            JProperty? prop_title = responseJO.Property("title");
            if (prop_title != null)
            {
                Title = prop_title.Value.ToString();
            }

            //description
            JProperty? prop_description = responseJO.Property("description");
            if (prop_description != null)
            {
                Description = prop_description.Value.ToString();
            }

            //Chapters
            JToken? chapters = responseJO.SelectToken("chapters");
            if (chapters != null)
            {
                List<Chapter> NewChapters = new List<Chapter>();
                foreach (JObject chap in (JArray)chapters)
                {
                    Chapter ThisChapter = new Chapter();
                    JProperty? chap_title = chap.Property("title");
                    if (chap_title != null)
                    {
                        ThisChapter.Title = chap_title.Value.ToString();
                    }
                    JProperty? chap_purpose = chap.Property("purpose");
                    if (chap_purpose != null)
                    {
                        ThisChapter.Purpose = chap_purpose.Value.ToString();
                    }
                    NewChapters.Add(ThisChapter);
                }
                Chapters = NewChapters.ToArray();
            }
        }        
    
    
    }
}