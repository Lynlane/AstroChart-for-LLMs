using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroTEXT.Models
{
    public class AspectSetting
    {
        public double Degree { get; set; }
        public double Orb { get; set; }
        public string DisplayText => $"{Degree}° (±{Orb}°)";
    }
}
