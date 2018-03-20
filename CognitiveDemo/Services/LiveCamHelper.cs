namespace CognitiveDemo
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.ProjectOxford.Face;

    using ServiceHelpers;

    public static class LiveCamHelper
    {
        public static bool IsFaceRegistered { get; set; }

        public static bool IsInitialized { get; set; }

        public static string WorkspaceKey
        {
            get;
            set;
        }

        public static Action<string> GreetingsCallback { get; set; }

        public static Action<CognitiveFaceResult> FacesDetectedCallback { get; set; }

        private const string PersonGroupId = "c6bee2f7-5cc1-446c-ba73-3457108edb7e";

        public static void Init(Action throttled = null)
        {
            FaceServiceHelper.ApiKey = "b7b0c6b7a17142229d436d31b11477a3";
            FaceServiceHelper.ApiKeyRegion = "westeurope";
            if(throttled!=null)
            FaceServiceHelper.Throttled += throttled;

            WorkspaceKey = Guid.NewGuid().ToString();
            ImageAnalyzer.PeopleGroupsUserDataFilter = WorkspaceKey;
            FaceListManager.FaceListsUserDataFilter = WorkspaceKey;

            IsInitialized = true;
        }

        public static async Task RegisterFaces()
        {

            try
            {
                var persongroupId = Guid.NewGuid().ToString();
                await FaceServiceHelper.CreatePersonGroupAsync(
                    persongroupId,
                    "Xamarin",
                    WorkspaceKey);

                await FaceServiceHelper.CreatePersonAsync(persongroupId, "Albert Einstein");

                var personsInGroup = await FaceServiceHelper.GetPersonsAsync(persongroupId);

                await FaceServiceHelper.AddPersonFaceAsync(
                    persongroupId,
                    personsInGroup[0].PersonId,
                    "https://upload.wikimedia.org/wikipedia/commons/d/d3/Albert_Einstein_Head.jpg", 
                    null, 
                    null);

                await FaceServiceHelper.TrainPersonGroupAsync(persongroupId);

                IsFaceRegistered = true;
            }
            catch (FaceAPIException ex)
            {
                //Console.WriteLine(ex.Message);
                IsFaceRegistered = false;
            }
        }

        public static async Task ProcessCameraCapture(ImageAnalyzer e, int imageId)
        {
            DateTime start = DateTime.Now;

            await e.DetectFacesAsync(true);
            //await e.DetectEmotionAsync();

            if (e.DetectedFaces.Any())
            {
                //var faces = e.DetectedFaces.Select(f => 
                //    new CognitiveFaceResult()
                //    {
                //        Gender = f.FaceAttributes.Gender[0].ToString().ToUpper() + f.FaceAttributes.Gender.Substring(1),
                //        Age = f.FaceAttributes.Age,
                //        Left = f.FaceRectangle.Left,
                //        Top = f.FaceRectangle.Top,
                //        Width = f.FaceRectangle.Width,
                //        Height = f.FaceRectangle.Height
                //    });

                //DisplayMessage(GetFaceAttributeString(e));
                
                var result = new CognitiveFaceResult();

                await e.IdentifyFacesAsync(PersonGroupId);

                result.Attributes = GetFaceAttributeString(e);

                if (e.IdentifiedPersons.Any())
                {
                    result.IdentitiedPersons = GetGreettingFromFaces(e);
                }
                else
                {
                    result.IdentitiedPersons = "No Idea, who you are... Register your face.";
                }

                FacesDetectedCallback.Invoke(result);

                //greetingsText = GetFaceAttributeString(e) + "\r\n" + greetingsText;

                //if (e.IdentifiedPersons.Any())
                //{
                //    if (GreetingsCallback != null)
                //    {
                //        DisplayMessage(greetingsText);
                //    }

                //    //Console.WriteLine(greetingsText);
                //}
                //else
                //{
                //    DisplayMessage("No Idea, who you are... Register your face.");

                //    //Console.WriteLine("No Idea");

                //}
            }
            else
            {
               // DisplayMessage("No face detected.");

                //Console.WriteLine("No Face ");

            }

            TimeSpan latency = DateTime.Now - start;
            var latencyString = string.Format("Face API latency: {0}ms", (int)latency.TotalMilliseconds);
            //Console.WriteLine(latencyString);
        }

        private static string GetFaceAttributeString(ImageAnalyzer imageAnalyzer)
        {
            var face = imageAnalyzer.DetectedFaces.First();

            var gender = face.FaceAttributes.Gender[0].ToString().ToUpper() + face.FaceAttributes.Gender.Substring(1);

            return $"{gender}, {face.FaceAttributes.Age} years old";
        }

        private static string GetGreettingFromFaces(ImageAnalyzer img)
        {
            if (img.IdentifiedPersons.Any())
            {
                string names = img.IdentifiedPersons.Count() > 1 ? string.Join(", ", img.IdentifiedPersons.Select(p => p.Person.Name)) : img.IdentifiedPersons.First().Person.Name;

                if (img.DetectedFaces.Count() > img.IdentifiedPersons.Count())
                {
                    return string.Format("I know you {0} and company!", names);
                }
                else
                {
                    return string.Format("I know you {0}!", names);
                }
            }
            else
            {
                if (img.DetectedFaces.Count() > 1)
                {
                    return "Hi everyone! If I knew any of you by name I would say it...";
                }
                else
                {
                    return "Hi there! If I knew you by name I would say it...";
                }
            }
        }

        static void DisplayMessage(string greetingsText)
        {
            GreetingsCallback?.Invoke(greetingsText);
        }
    }
}
