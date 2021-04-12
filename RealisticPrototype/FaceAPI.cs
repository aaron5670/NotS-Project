using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealisticPrototype
{
    class FaceAPI
    {
        static string personGroupId = Guid.NewGuid().ToString();

        const string SUBSCRIPTION_KEY = "70ff5e1b8d664f858873f5d9df71a92a";
        const string ENDPOINT = "https://nots.cognitiveservices.azure.com/";
        const string RECOGNITION_MODEL3 = RecognitionModel.Recognition03;
        private IFaceClient client;

        public FaceAPI()
        {
            client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);

            trainGroup();
        }

        /*
         *	AUTHENTICATE
         *	Uses subscription key and region to create a client.
         */
        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        private static async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, Stream stream, string recognition_model)
        {
            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithStreamAsync(stream, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection02);
            Console.WriteLine($"{detectedFaces.Count} face(s) detected from image");
            return detectedFaces.ToList();
        }

        private async Task trainGroup(string recognitionModel = RECOGNITION_MODEL3)
        {
            Dictionary<string, string[]> personDictionary = new Dictionary<string, string[]>{
                { "Bill Gates", new[] { "gates.jpg", "gates2.jpg" } },
                { "Steve Jobs", new[] { "jobs.jpg", "jobs2.jpg" } },
                { "Mark Rutte", new[] { "rutte.jpg", "rutte2.jpg" } },
                { "Keanu Reeves", new[] { "wick.jpg", "wick2.jpg" } }
            };

            await client.PersonGroup.CreateAsync(personGroupId, personGroupId, recognitionModel: recognitionModel);
            foreach (var groupedFace in personDictionary.Keys)
            {
                await Task.Delay(250);
                Person person = await client.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: groupedFace);
                Console.WriteLine($"Create a person group person '{groupedFace}'.");

                // Add face to the person group person.
                foreach (var similarImage in personDictionary[groupedFace])
                {
                    Console.WriteLine($"Add face to the person group person({groupedFace}) from image `{similarImage}`");
                    string path = System.AppDomain.CurrentDomain.BaseDirectory;
                    PersistedFace face = await client.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, person.PersonId, File.OpenRead($"{path}/../../images/{similarImage}"));
                }
            }
            // Start to train the person group.
            //output("");
            Console.WriteLine($"Train person group {personGroupId}.");
            await client.PersonGroup.TrainAsync(personGroupId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(personGroupId);
                Console.WriteLine($"Training status: {trainingStatus.Status}.");
                if (trainingStatus.Status == TrainingStatusType.Succeeded) { break; }
            }
        }

        public async Task IdentifyInPersonGroup(Stream recognizeStream, Action<string> output, string recognitionModel = RECOGNITION_MODEL3)
        {
            output("");
            List<Guid> sourceFaceIds = new List<Guid>();
            // Detect faces from source image url.
            List<DetectedFace> detectedFaces = await DetectFaceRecognize(client, recognizeStream, recognitionModel);

            // Add detected faceId to sourceFaceIds.
            foreach (var detectedFace in detectedFaces) { sourceFaceIds.Add(detectedFace.FaceId.Value); }
            // Identify the faces in a person group. 
            var identifyResults = await client.Face.IdentifyAsync(sourceFaceIds, personGroupId);

            foreach (var identifyResult in identifyResults)
            {
                Person person = await client.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
                output($"Person '{person.Name}' is identified for face in: image - {identifyResult.FaceId}," +
                    $" confidence: {identifyResult.Candidates[0].Confidence}.");
            }
            output("");
        }
    }
}
