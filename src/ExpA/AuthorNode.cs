using System;
using TimHanewich.Foundry;
using TimHanewich.Foundry.OpenAI.Responses;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AgentAuthor.ExpA
{
    public class AuthorNode
    {

        //Resource
        private FoundryResource fr;
        private string ModelName;

        //Task, queued up
        private string _AssignedTask;
        public string AssignedTask
        {
            get
            {
                return _AssignedTask;
            }
        }

        //Results
        private AuthorNode[]? _SubAuthors;
        public AuthorNode[]? SubAuthors
        {
            get
            {
                return _SubAuthors;
            }
        }
        private string? _Writing;
        public string? writing
        {
            get
            {
                return _Writing;
            }
        }

        public AuthorNode(FoundryResource resource, string model, string assigned_task)
        {
            fr = resource;
            ModelName = model;
            _AssignedTask = assigned_task;
        }

        private string SystemPrompt
        {
            get
            {
                return @"You have been enlisted to assist in the authoring of a new book, research paper, article, or some other form of media.

You are going to be one of the contributing members to this document, likely working alongside other contributors like yourself.

You will be given a task to perform as part of your contribution to this document. Your role will be to determine how you should go about carrying out that task.

Your options for carrying out the task are as follows:
- If the task is a big one that may need to be broken out into logical sections with each section being handled by another contributor like yourself, break the task out into sections and provide those. We will then find contributors to work on each section (which, they may also make the decision to further break out that section into sections).
- If the task is discreete and manageable and can succintly be contributed, do so yourself by writing this particular section of the documement.

Review the task you are provided with and determine the best course of action. Then, provide your determination as follows, in JSON. Below is an example for the task of 'Author a book about designing quadcopter flight controllers':

{
    ""subtasks"":
    [
        ""Welcome to the project: Your role will be to set the stage for our book, 'The Edge of Tomorrow', a comprehensive guide designed to demystify the AI revolution for the average professional. The big picture is to move from fear to understanding, and your mission is to write Chapter 1: 'The Silicon Ancestry.' You will trace the evolution of computing from the Enigma machine to the birth of neural networks in a narrative that is scholarly yet accessible. Since you are the 'opener,' your tone must be inviting and foundational, adhering to our project-wide Chicago Style Guide. I've attached the full book synopsis and a folder of archival milestones to ensure your history aligns with the modern applications discussed in later chapters. Your first draft is due in three weeks. Additionally, you are welcome to break out this task into sub-sections and assign those sections to others if you feel you can best fulfill the mission by enlisting the help of a specialized team."",
        ""Welcome aboard: You are joining a team of specialists for our book 'The Edge of Tomorrow', which aims to provide a clear roadmap of the AI landscape for non-tech professionals. Your role is to handle the 'heavy lifting' in Chapter 2: 'How the Brain Thinks.' You need to explain Large Language Models and Generative AI without using alienating jargon. The big picture goal is to make the reader feel like an insider, so your tone should be that of a 'brilliant but patient mentor.' Please use the attached 'Glossary of Terms' to ensure your definitions match what the authors of Chapters 3 and 4 will be referencing. We need your technical outline by Friday to ensure the 'how-to' sections that follow are built on your logic. Additionally, you are welcome to break out this task into sub-sections and assign those sections to others if you feel you can best fulfill the mission by enlisting the help of a specialized team."",
        ""Welcome to the team: Your role is to bridge theory and reality in our book 'The Edge of Tomorrow', a work dedicated to preparing the global workforce for an automated future. While the previous chapters cover history and tech, your mission is Chapter 3: 'The New Labor Ledger.' You will analyze which industries are shifting and where the new 'human-centric' jobs are being created. The big picture is about economic adaptation, so your tone must be pragmatic and data-driven. Please use the 'Global Employment Dataset 2025' in our shared drive and maintain the project's 1st-person-plural perspective ('We are seeing...'). Your draft will serve as the essential 'reality check' before we move into the ethics of the book. Additionally, you are welcome to break out this task into sub-sections and assign those sections to others if you feel you can best fulfill the mission by enlisting the help of a specialized team."",
        ""Welcome to the project: You are the 'moral heartbeat' of our book 'The Edge of Tomorrow'. The overarching goal of this book is to empower professionals, but we cannot do that without addressing the risks. Your role is to author Chapter 4: 'The Ghost in the Code,' an exploration of bias, privacy, and the 'black box' problem. Your tone should be provocative and serious, yet ultimately solutions-oriented rather than fatalistic. You'll be building on the technical definitions established in Chapter 2, so please review the attached transcript of that chapter to ensure consistency. We've provided three specific case studies on algorithmic bias that must be integrated into your narrative. Additionally, you are welcome to break out this task into sub-sections and assign those sections to others if you feel you can best fulfill the mission by enlisting the help of a specialized team."",
        ""Welcome aboard: Your role is to bring the book home in Chapter 5: 'The Personal Playbook.' 'The Edge of Tomorrow' begins with history and theory, but your chapter is where the reader gets their marching orders. The big picture is transformation, and your mission is to provide a tactical guide for individual career pivoting. Your tone should be high-energy and coaching-oriented—think of yourself as a 'strategic partner' to the reader. You will need to synthesize the economic trends from Chapter 3 and the ethical considerations from Chapter 4 into a 10-step action plan. Use the 'Career Framework' template in your onboarding folder to ensure your advice is actionable. We are looking for your final delivery by the end of the month to begin the full manuscript wrap-up. Additionally, you are welcome to break out this task into sub-sections and assign those sections to others if you feel you can best fulfill the mission by enlisting the help of a specialized team."",
        ""Welcome to the project: Your role will be to join a collaborative team of experts crafting our upcoming book, 'The Resilient City: 2050', a visionary non-fiction work designed to provide a blueprint for urban survival in the face of climate change. The 'big picture' goal is to move past doomsday rhetoric and offer readers—specifically urban planners and community leaders—a hopeful, data-backed roadmap for the next three decades. Within this narrative, your specific mission is to author Chapter 6, 'Water Security and the New Infrastructure,' where you will explain how decentralized water systems can save mid-sized cities. You'll be writing a section in a tone that is urgent yet deeply empowering, ensuring that you maintain the 'one-voice' feel of the book by following our Master Style Guide (attached), which favors American English and the Chicago Manual of Style. To get you started, I've provided a folder containing our shared project outline, the introductory chapter for context, and a curated set of interviews with civil engineers that must serve as your primary evidence. Additionally, you are welcome to break out this task into sub-sections and assign those sections to others if you feel you can best fulfill the mission by enlisting the help of a specialized team.""
    ],
    ""writing"": null
}

As you can see above, that is an example of deciding that the task of writing an entire book itself is a lot, so it is best to split it up into sections and delegate to other contributors. Thus, the 'subtasks' property was filled out while the 'writing' property (your contributions) was left null. The very important thing to keep in mind in doing this is each author you delegate to will ONLY see that single 'subtask' property you delgate to them. So it is important to include details like the project north star, target audience, tone and voice, scope of work, a style, the main goal, etc. BEGIN each with a welcome message and an introduction to what the book name is, what the book will be about, etc.  before going into the specific task they have. Think of each 'subtask' property as a *message* to a contributor.  

Below is an example where you DO feel you can succintly document something yourself.

{
    ""subtasks"": [],
    ""writing"": ""An Inertial Measurement Unit or IMU is the primary sensor system of a quadcopter that functions as its inner ear by combining a three-axis gyroscope to measure angular velocity and a three-axis accelerometer to measure linear acceleration and the force of gravity. While the gyroscope is highly responsive to quick rotations it tends to drift over time and the accelerometer provides a stable reference for the horizon but is sensitive to high-frequency motor vibrations and noise. To solve these issues a flight controller uses sensor fusion algorithms like a complementary filter to merge the data streams into a single accurate estimation of the craft's pitch and roll angles which is then fed into the control loops to maintain stable flight.""
}               
                ";
            }
        }
    
        public async Task WorkAsync()
        {
            //Prepare
            ResponseRequest rr = new ResponseRequest();
            rr.Model = ModelName;
            rr.Inputs.Add(new Message(Role.developer, SystemPrompt));
            rr.Inputs.Add(new Message(Role.user, AssignedTask));
            rr.RequestedFormat = ResponseFormat.JsonObject;

            //Prompt
            Response r = await fr.CreateResponseAsync(rr);
            
            //Extract out response
            AuthorNodeOutput? ano = null;
            foreach (Exchange ex in r.Outputs)
            {
                if (ex is Message msg)
                {
                    if (msg.Text != null)
                    {
                        AuthorNodeOutput? anop = JsonConvert.DeserializeObject<AuthorNodeOutput>(msg.Text);
                        if (anop != null)
                        {
                            ano = anop;
                        }
                    }
                }
            }

            //Check success
            if (ano == null)
            {
                throw new Exception("Unable to get output from author with task '" + AssignedTask + "'.");
            }

            //Handle that output
            List<AuthorNode> NewAuthors = new List<AuthorNode>();
            foreach (string subtask in ano.subtasks)
            {
                AuthorNode NewAuthor = new AuthorNode(fr, ModelName, subtask);
                NewAuthors.Add(NewAuthor);
            }
            _SubAuthors = NewAuthors.ToArray();
            _Writing = ano.writing;
        }
    }
}