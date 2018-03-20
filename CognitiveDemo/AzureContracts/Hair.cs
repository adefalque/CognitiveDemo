using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveDemo.AzureContracts
{
    public class Hair
    {
        public bool Invisible { get; set; }
        public string[] HairColor { get; set; }

        public Hair(Microsoft.ProjectOxford.Face.Contract.Hair hair)
        {
            this.Invisible = hair.Invisible;
            this.HairColor = hair.HairColor.Select(h => h.Color.ToString()).ToArray();
        }
    }
}
