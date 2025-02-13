using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroTEXT.Models;

namespace AstroTEXT.Services
{
    public class AspectCalculator
    {
        public static List<string> CalculateAspects(List<CelestialBody> bodies, List<AspectSetting> settings)
        {
            var aspects = new List<string>();
            var pairs = bodies.SelectMany((x, i) => bodies.Skip(i + 1), (x, y) => (x, y));

            foreach (var (a, b) in pairs)
            {
                foreach (var set in settings)
                {
                    double angle = Math.Abs(a.Longitude - b.Longitude);
                    angle = angle > 180 ? 360 - angle : angle;

                    if (Math.Abs(angle - set.Degree) <= set.Orb)
                    {
                        bool isApplying = (a.Speed > b.Speed);
                        aspects.Add($"{a.Name} 和 {b.Name} 形成 {set.Degree}°相位 " +
                                   $"（{(isApplying ? "入" : "出")}相位，误差 {Math.Abs(angle - set.Degree):F2}°）");
                    }
                }
            }
            return aspects;
        }
    }
}
