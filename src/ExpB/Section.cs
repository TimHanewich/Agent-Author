using System;
using TimHanewich.Foundry.OpenAI.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public async Task WriteAsync(Chapter chap, Book b)
        {
            string prompt = @"
            
You are on the team of contributors for a new book titled '" + b.Title + @"'

The description of this book is: " + b.Description + @"

Your goal is to help contribute to a particular chapter of the book by writing one of the sections in this chapter.

The chapter you will work on is titled '" + chap.Title + @"'

The purpose of this chapter is: " + chap.Purpose + @"'

The section of this chapter you will be writing has the heading '" + Heading + @"'

The goal of this section is '" + Goal + @"'

You will be asked to write this section of the chapter. When you do, always provide your response in JSON format, like this as an example:

{
    ""section_content"": ""The modern marketplace is witnessing a significant pivot away from the era of mass-produced uniformity. For decades, the goal of manufacturing was perfection through repetition, ensuring that every product off the assembly line was identical to the last. However, as automation makes this level of perfection cheap and common, a new value is being placed on the human signature. The craftsmanship revival is not about a rejection of technology, but a relocation of value toward the creator’s intent. When an individual chooses an artisanal leather bag or a hand-forged kitchen knife, they are purchasing more than a utility; they are acquiring a narrative of labor, skill, and time. This human element acts as a bridge between the producer and the consumer, creating a sense of soul that a purely digital or robotic process cannot manufacture. \n\nThis movement is particularly visible in industries where the tactile experience is central to the product. In the world of high-end horology, for example, mechanical watches have seen a massive resurgence despite the fact that a digital device keeps time more accurately. The appeal lies in the complexity of the hand-assembled gears and the knowledge that a master watchmaker spent weeks tuning the movement. Similarly, the specialty coffee industry has shifted the focus from a quick caffeine hit to a sensory ritual involving specific origins and manual brewing methods. In both cases, the value is derived from the deliberate slowing down of the process.\n\nFor businesses and creators, the takeaway from this revival is clear: authenticity is the new currency. In a world where AI can generate infinite variations of a design in seconds, the evidence of human effort—the slight irregularity in a ceramic glaze or the unique grain in a piece of hand-planed wood—becomes a mark of quality. These imperfections serve as a signal of reality in a sea of synthetic outputs. By emphasizing the craftsmanship behind a project, a brand can transcend the commodity trap and build a deeper, more resilient connection with an audience that is hungry for something real.""
}

As you can see above, your role is write that particular section.

            ";

            //Load configuration
            Configuration config = Configuration.LoadDefault();

            //Draft request
            ResponseRequest rr = new ResponseRequest();
            rr.Model = config.ModelName;
            rr.Inputs.Add(new Message(Role.user, prompt));
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

            //Get the section_content
            JProperty? prop_section_content = responseJO.Property("section_content");
            if (prop_section_content != null)
            {
                Content = prop_section_content.Value.ToString();
            }
        }
    }
}