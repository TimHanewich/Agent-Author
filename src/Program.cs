using System;
using System.Net;
using Newtonsoft.Json;
using TimHanewich.Foundry;
using TimHanewich.Foundry.OpenAI.Responses;

namespace AgentAuthor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AuthorBookAsync().Wait();
        }

        public static async Task AuthorBookAsync()
        {
            Configuration config = Configuration.LoadDefault();
            
            Console.WriteLine("Describe the book you want me to write.");
            Console.Write("> ");
            string? desc = Console.ReadLine();
            if (desc == null)
            {
                return;
            }

            //Set up variables for tracking statistics
            DateTime StartedAt = DateTime.Now;
            int CumulativeInputTokensConsumed = 0;
            int CumulativeOutputTokensConsumed = 0;

            //Create the book plan
            Console.WriteLine();
            Console.Write("Planning book structure... ");
            Book b = new Book();
            InferenceResult BookPlanningConsumption = await b.PlanBookAsync(desc);
            CumulativeInputTokensConsumed = CumulativeInputTokensConsumed + BookPlanningConsumption.InputTokensConsumed;
            CumulativeOutputTokensConsumed = CumulativeOutputTokensConsumed + BookPlanningConsumption.OutputTokensConsumed;
            Console.WriteLine("Done! " + b.Chapters.Length.ToString() + " chapters planned.");
            Console.WriteLine();
            Console.WriteLine("Book Title: " + b.Title);
            Console.WriteLine("Book Description: " + b.Description);
            Console.WriteLine("Book Chapters: ");
            foreach (Chapter chap in b.Chapters)
            {
                Console.WriteLine("- " + chap.Title);
            }

            //Draft out each chapter
            Console.WriteLine();
            foreach (Chapter chap in b.Chapters)
            {
                Console.Write("Planning structure for chapter '" + chap.Title + "'... ");
                InferenceResult ChapterPlanningConsumption = await chap.PlanChapterAsync(b);
                CumulativeInputTokensConsumed = CumulativeInputTokensConsumed + ChapterPlanningConsumption.InputTokensConsumed;
                CumulativeOutputTokensConsumed = CumulativeOutputTokensConsumed + ChapterPlanningConsumption.OutputTokensConsumed;
                Console.WriteLine(chap.Sections.Length.ToString() + " sections planned.");

                //Write each section
                foreach (Section sect in chap.Sections)
                {
                    Console.Write("Writing section '" + sect.Heading + "'... ");
                    InferenceResult SectionWritingConsumption = await sect.WriteAsync(chap, b);
                    CumulativeInputTokensConsumed = CumulativeInputTokensConsumed + SectionWritingConsumption.InputTokensConsumed;
                    CumulativeOutputTokensConsumed = CumulativeOutputTokensConsumed + SectionWritingConsumption.OutputTokensConsumed;
                    Console.WriteLine(sect.Content.Length.ToString("#,##0") + " characters written!");
                }
                Console.WriteLine();
            }

            //Stop the clock
            TimeSpan ElapsedInferenceTime = DateTime.Now - StartedAt;

            //Export it
            string ExportPath = @"C:\Users\timh\Downloads\Agent-Author\export.md";
            Console.Write("Exporting... ");
            ExportBook(b, ExportPath);
            Console.WriteLine("Exported!");

            //Print statistics
            Console.WriteLine();
            Console.WriteLine("Elapsed inference time: " + ElapsedInferenceTime.TotalMinutes.ToString("#,##0.0") + " minutes.");
            Console.WriteLine("Cumulative Input Tokens: " + CumulativeInputTokensConsumed.ToString("#,##0"));
            Console.WriteLine("Cumulative Output Tokens Consumed: " + CumulativeOutputTokensConsumed.ToString("#,##0"));
        }

        public static void ExportBook(Book b, string ExportPath)
        {
            
            string FULL = "";
            
            //Title
            FULL = "# " + b.Title + Environment.NewLine + b.Description;

            //Table of Contents
            FULL = FULL + Environment.NewLine + Environment.NewLine + "## TABLE OF CONTENTS" + Environment.NewLine;
            foreach (Chapter chap in b.Chapters)
            {
                FULL = FULL + "- " + chap.Title + Environment.NewLine;
            }
            
            //Do each chapter
            FULL = FULL + Environment.NewLine;
            foreach (Chapter chap in b.Chapters)
            {
                
                //Chapter title
                FULL = FULL + "## " + chap.Title + Environment.NewLine + Environment.NewLine;

                //Do each section
                foreach (Section sect in chap.Sections)
                {
                    FULL = FULL + "### " + sect.Heading + Environment.NewLine + sect.Content + Environment.NewLine + Environment.NewLine;
                }

                //Line after chapter
                FULL = FULL + Environment.NewLine;
            }

            //Export!
            System.IO.File.WriteAllText(ExportPath, FULL);

        }
    
    }
}