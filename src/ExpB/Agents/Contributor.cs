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

        public async Task<Book> PlanBookAsync(string desired_book_info)
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
            //Draft request
            ResponseRequest rr = new ResponseRequest();
            rr.Model = ModelName;
            rr.Inputs.Add(new Message(Role.developer, prompt));
            rr.Inputs.Add(new Message(Role.user, desired_book_info));
            rr.RequestedFormat = ResponseFormat.JsonObject;
            
            //request
            Response r = await Foundry.CreateResponseAsync(rr);

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

            //Prepare to return
            Book ToReturn = new Book();

            //Title
            JProperty? prop_title = responseJO.Property("title");
            if (prop_title != null)
            {
                ToReturn.Title = prop_title.Value.ToString();
            }

            //description
            JProperty? prop_description = responseJO.Property("description");
            if (prop_description != null)
            {
                ToReturn.Description = prop_description.Value.ToString();
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
                ToReturn.Chapters = NewChapters.ToArray();
            }

            return ToReturn;
        }

        public async Task PlanChapterAsync(Chapter chap, Book b) //book provided as reference
        {
            string prompt = @"
            
You have just joined the contributing team for a new book titled '" + b.Title + @"'. 

The book is described as the following: " + b.Description + @"

Your particular goal is to help plan out a particular chapter within that book. To be specific, your role is to determine how a chapter should be written by defining what sections it should have, and what the goal of each of those sections is.

Always return your answer in JSON, like below for example:

{
    ""sections"":
    [
        {
            ""heading"": ""The Luxury of Tangibility"",
            ""goal"": ""This section explains the economic shift where physical objects are no longer seen as outdated but as premium assets. The writer should focus on how digital abundance has made physical possession feel more exclusive. The goal is to show that when everything is available on a screen for free, people are willing to pay a high price for something they can hold, smell, and keep on a shelf.""
        },
        {
            ""heading"": ""The Craftsmanship Revival"",
            ""goal"": ""This section explores the return to traditional methods of production in industries like watchmaking, woodworking, and specialty food. The writer needs to highlight the human element of the creator and the story behind the object. The goal is to prove that consumers find more value in items that show evidence of human effort and imperfection than in perfectly uniform, machine-made products.""
        },
        {
            ""heading"": ""Psychology of the Senses"",
            ""goal"": ""This section provides the scientific backing for why analog experiences are more memorable. The writer should reference studies on haptic memory and how the brain processes information differently when it is associated with a physical sensation like the turning of a page. The goal is to convince the reader that tactile interaction leads to deeper learning and a stronger emotional connection to the content.""
        }
    ]
}

As you can see above, define each section that should be within this chapter, along with the goal of each section (why it belongs in the chapter and how it contributes to the overall goal of the book).
";
        
            //Plan user prompt
            string user_prompt = "Please help me plan out the following chapter for my book '" + b.Title + "': " + "\n\n" + "Chapter Title: " + chap.Title + "\n\n" + "Purpose of this chapter: " + chap.Purpose;

            //Draft request
            ResponseRequest rr = new ResponseRequest();
            rr.Model = ModelName;
            rr.Inputs.Add(new Message(Role.developer, prompt));
            rr.Inputs.Add(new Message(Role.user, user_prompt));
            rr.RequestedFormat = ResponseFormat.JsonObject;
            
            //request
            Response r = await Foundry.CreateResponseAsync(rr);

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
        
            //Sections
            JToken? sections = responseJO.SelectToken("sections");
            if (sections != null)
            {
                List<Section> CollectedSessions = new List<Section>();
                JArray sectionsJA = (JArray)sections;
                foreach (JObject sectJO in sectionsJA)
                {
                    Section s = new Section();
                    JProperty? prop_heading = sectJO.Property("heading");
                    if (prop_heading != null)
                    {
                        s.Heading = prop_heading.Value.ToString();
                    }
                    JProperty? prop_goal = sectJO.Property("goal");
                    if (prop_goal != null)
                    {
                        s.Goal = prop_goal.Value.ToString();
                    }
                    CollectedSessions.Add(s);
                }
                chap.Sections = CollectedSessions.ToArray();
            }
        }
        
    }
}