using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CognitiveDemo;

[assembly: Xamarin.Forms.Dependency(typeof(FaceService))]

namespace CognitiveDemo
{
    using System.IO;
    using System.Threading;

    using Microsoft.ProjectOxford.Face;
    using Microsoft.ProjectOxford.Face.Contract;

    using ServiceHelpers;

    using Person = CognitiveDemo.AzureContracts.Person;

    public class FaceService : IFaceService
    {
        private const string PersonGroupId = "c6bee2f7-5cc1-446c-ba73-3457108edb7e";

        private const string ApiKey = "b7b0c6b7a17142229d436d31b11477a3";

        private const string ApiUrl = "https://westeurope.api.cognitive.microsoft.com/face/v1.0";

        private FaceServiceClient faceServiceClient = null;

        private readonly FaceAttributeType[] faceAttributes = new FaceAttributeType[]
                                                                  {
                                                                      FaceAttributeType.Age,
                                                                      FaceAttributeType.Gender,
                                                                      FaceAttributeType.Hair,
                                                                      FaceAttributeType.Emotion
                                                                  };

        private Person[] knownPersons;

        public FaceService()
        {
            this.faceServiceClient = new FaceServiceClient(ApiKey, ApiUrl);
        }

        private SemaphoreSlim lockObject = new SemaphoreSlim(1);
        public async Task<Person[]> GetKnownPersons(bool useCache = true)
        {
            try
            {
                await this.lockObject.WaitAsync();

                if (this.knownPersons == null || !useCache)
                {
                    var persons = await this.faceServiceClient.ListPersonsAsync(PersonGroupId);

                    if (persons != null)
                    {
                        this.knownPersons = persons.Select(p => new Person() { Name = p.Name, PersonId = p.PersonId })
                            .ToArray();
                    }
                }

                return this.knownPersons;
            }
            finally
            {
                this.lockObject.Release();
            }
        }

        public async Task<FaceIdentificationResult> IdentityFace(Stream imageStream)
        {
            Face[] faces = null;

            try
            {
                faces = await this.faceServiceClient.DetectAsync(imageStream, true, false, this.faceAttributes);
            }
            catch (Exception e)
            {
                return new FaceIdentificationResult() { HasDetectionError = true };
            }

            if (faces.Length == 0)
            {
                return new FaceIdentificationResult() { HasDetectionError = true };
            }

            try
            {
                var face = faces[0];

                var faceIds = new Guid[] { face.FaceId };

                var results = await this.faceServiceClient.IdentifyAsync(PersonGroupId, faceIds, 1);

                var personName = "";
                if (results.Length == 1 && results[0].Candidates.Length > 0)
                {
                    var cachedKnownPersons = await this.GetKnownPersons();

                    var person = cachedKnownPersons.FirstOrDefault(p => p.PersonId == results[0].Candidates[0].PersonId);

                    if (person != null)
                    {
                        personName = person.Name;
                    }
                }

                return new FaceIdentificationResult(face, personName);

            }
            catch (Exception)
            {
                return new FaceIdentificationResult() { HasDetectionError = true };
            }
        }
    }
}
