using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Rekognition.Model;
using AWSModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvokeAWSLambda
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IAmazonLambda client = new AmazonLambdaClient();

            PayLoad payload = new PayLoad();

            foreach (var imgFile in Directory.GetFiles(@"C:\Users\Olsv\Desktop\Images", "*.jpg"))
                payload.ValidationImages.Add(new ValidationImage() { Path = imgFile, ImageBytes = File.ReadAllBytes(imgFile) });

            var response = await client.InvokeAsync(
                new InvokeRequest
                {
                    FunctionName = "AWSLambdaOlleBolle",
                    Payload = JsonSerializer.Serialize(payload),
                });

            var result = JsonSerializer.Deserialize<List<ScannedImage>>(response.Payload.ToArray());
            //Encoding.ASCII.GetString(ms.ToArray());

            foreach (var item in result)
            {
                Console.WriteLine("------------------------------\n\n" + item.Path);
                Console.WriteLine("Image is approved: " + item.IsApproved);

                if (!item.IsApproved)
                {
                    Console.WriteLine("Reason: " + item.NotApprovedReason);
                }

                if(item.Labels.Count > 0)
                {
                    Console.WriteLine("\nLabels:");
                    foreach (var label in item.Labels)
                    {
                        Console.WriteLine(" " + label);
                    }
                }

                if (item.ModerationLabels.Count > 0)
                {
                    Console.WriteLine("\nModeration Labels:");
                    foreach (var label in item.ModerationLabels)
                    {
                        Console.WriteLine(" " + label);
                    }
                }

                Console.WriteLine("\n----------------------------\n\n");
            }
            Console.ReadLine();
        }
        private static IConfiguration SetConfig()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            return config;
        }
    }
}
