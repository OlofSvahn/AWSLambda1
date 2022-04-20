using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using AWSModels;
using Microsoft.Extensions.Configuration;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda1
{
    public class Function
    {
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// 

        private readonly AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient();

        private IConfiguration config;

        public async Task<List<ScannedImage>> FunctionHandler(PayLoad payload, ILambdaContext context)
        {
            List<ScannedImage> scannedImagesList = new List<ScannedImage>();
            List<Task> tasksList = new List<Task>();

            foreach (var item in payload.ValidationImages)
                tasksList.Add(ProcessImage(item, scannedImagesList));

            await Task.WhenAll(tasksList);

            return scannedImagesList;
        }

        public async Task ProcessImage(ValidationImage item, List<ScannedImage> scannedImagesList)
        {
            Image image = new Image() { Bytes = new MemoryStream(item.ImageBytes) };
            ScannedImage scannedImage = new ScannedImage() { Path = item.Path };

            if (!await DetectFaces(image, scannedImage))
                scannedImage.IsApproved = false;
            else if (!await DetectLabels(image, scannedImage))
                scannedImage.IsApproved = false;
            else if (!await DetectModerationLabels(image, scannedImage))
                scannedImage.IsApproved = false;
            else
                scannedImage.IsApproved = true;

            scannedImagesList.Add(scannedImage);
        }

        public async Task<bool> DetectFaces(Image image, ScannedImage scannedImage)
        {
            DetectFacesRequest detectFacesRequest = new DetectFacesRequest()
            {
                Image = image,
            };

            try
            {
                DetectFacesResponse detectFacesResponse = await rekognitionClient.DetectFacesAsync(detectFacesRequest);
                List<FaceDetail> faces = detectFacesResponse.FaceDetails;

                if (faces.Count == 0)
                {
                    scannedImage.NotApprovedReason = "Image does not contain human face";
                    return false;
                }
                else
                {
                    foreach (FaceDetail face in detectFacesResponse.FaceDetails)
                    {
                        scannedImage.FaceDetails.Add("Confidence", face.Confidence);
                        scannedImage.FaceDetails.Add("Landmarks", face.Landmarks.Count);
                        scannedImage.FaceDetails.Add("Pitch", face.Pose.Pitch);
                        scannedImage.FaceDetails.Add("Roll", face.Pose.Roll);
                        scannedImage.FaceDetails.Add("Yaw", face.Pose.Yaw);
                        scannedImage.FaceDetails.Add("AgeRangeLow", face.AgeRange.Low);
                        scannedImage.FaceDetails.Add("AgeRangeHigh", face.AgeRange.High);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return true;
        }

        public async Task<bool> DetectLabels(Image image, ScannedImage scannedImage)
        {
            DetectLabelsRequest detectlabelsRequest = new DetectLabelsRequest()
            {
                Image = image,
                MaxLabels = int.Parse(config["App:DetectLabels:LabelsMaxLabels"]),
                MinConfidence = float.Parse(config["App:DetectLabels:LabelsMinConfidence"]),
            };

            try
            {
                DetectLabelsResponse detectLabelsResponse = await rekognitionClient.DetectLabelsAsync(detectlabelsRequest);

                foreach (Label label in detectLabelsResponse.Labels)
                    scannedImage.Labels.Add(label.Name, label.Confidence);

                scannedImage.Labels = scannedImage.Labels.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                if (!CheckLabelValues(scannedImage))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return true;
        }

        public async Task<bool> DetectModerationLabels(Image image, ScannedImage scannedImage)
        {
            DetectModerationLabelsRequest detectModerationRequest = new DetectModerationLabelsRequest()
            {
                Image = image,
                MinConfidence = 40F,
            };

            try
            {
                DetectModerationLabelsResponse detectModerationResponse = await rekognitionClient.DetectModerationLabelsAsync(detectModerationRequest);

                foreach (ModerationLabel label in detectModerationResponse.ModerationLabels)
                    scannedImage.ModerationLabels.Add(label.Name, label.Confidence);

                scannedImage.ModerationLabels = scannedImage.ModerationLabels.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                if (!CheckModerationValues(scannedImage))
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return true;
        }

        public bool CheckLabelValues(ScannedImage scannedImage)
        {
            foreach (var label in scannedImage.Labels)
            {
                if (!scannedImage.Labels.ContainsKey("Face") || !scannedImage.Labels.ContainsKey("Person") || !scannedImage.Labels.ContainsKey("Human"))
                {
                    scannedImage.NotApprovedReason = "Image labels indicates no human face in picture";
                    return false;
                }

                if((label.Key == "Face" || label.Key == "Human") && label.Value < 80)
                {
                    scannedImage.NotApprovedReason = "Image label" + label.Key + " has value: " +  label.Value + " which is too low";
                    return false;
                }
            }
            return true;
        }

        public bool CheckModerationValues(ScannedImage scannedImage)
        {
            return true;
        }
    }
}
