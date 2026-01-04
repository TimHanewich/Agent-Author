using System;
using TimHanewich.Foundry.OpenAI.Responses;
using Newtonsoft.Json.Linq;

namespace AgentAuthor
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

        public async Task<InferenceResult> PlanChapterAsync(Book b) //book provided as reference
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
            string user_prompt = "Please help me plan out the following chapter for my book '" + b.Title + "': " + "\n\n" + "Chapter Title: " + Title + "\n\n" + "Purpose of this chapter: " + Purpose;

            //Load config
            Configuration config = Configuration.LoadDefault();

            //Draft request
            ResponseRequest rr = new ResponseRequest();
            rr.Model = config.ModelName;
            rr.Inputs.Add(new Message(Role.developer, prompt));
            rr.Inputs.Add(new Message(Role.user, user_prompt));
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
                Sections = CollectedSessions.ToArray();
            }

            //Return inference result
            InferenceResult ToReturn = new InferenceResult();
            ToReturn.InputTokensConsumed = r.InputTokensConsumed;
            ToReturn.OutputTokensConsumed = r.OutputTokensConsumed;
            return ToReturn;
        }
    }
}