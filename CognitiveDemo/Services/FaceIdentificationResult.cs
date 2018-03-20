using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveDemo
{
    using Microsoft.ProjectOxford.Face.Contract;

    using Hair = CognitiveDemo.AzureContracts.Hair;

    public class FaceIdentificationResult
    {
        public bool IsKnown { get; set; }

        public string PersonName { get; set; }

        public string Gender { get; set; }

        public double Age { get; set; }

        public bool HasDetectionError { get; set; }

        public string Hair { get; set; }

        public string Emotion { get; set; }

        public FaceIdentificationResult()
        {
            
        }

        public FaceIdentificationResult(Face face, string personName)
        {
            Gender = face.FaceAttributes.Gender[0].ToString().ToUpper() + face.FaceAttributes.Gender.Substring(1);
            Age = face.FaceAttributes.Age;
            PersonName = personName;

            IsKnown = !string.IsNullOrEmpty(personName);

            var hairVisible = face.FaceAttributes.Hair.Invisible ? "no" : "yes";
            var hairDescription = 
                face.FaceAttributes.Hair.HairColor.Length > 0 ?
                string.Join(
                ", ",
                face.FaceAttributes.Hair.HairColor.OrderByDescending(h => h.Confidence).First().Color.ToString()) : "";
            this.Hair = $"Hairs: {hairVisible} - {hairDescription}";
            this.Emotion = string.Join(
                ", ",
                face.FaceAttributes.Emotion.ToRankedList().OrderByDescending(e => e.Value).Take(2)
                    .Select(e => $"{e.Key} ({e.Value * 100})"));
        }
    }
}
