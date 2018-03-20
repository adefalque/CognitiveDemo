using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHelpers
{
    public class CognitiveFaceResult
    {
        public string Attributes { get; set; }

        public string IdentitiedPersons { get; set; }

        public string Gender { get; set; }

        public double Age { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
