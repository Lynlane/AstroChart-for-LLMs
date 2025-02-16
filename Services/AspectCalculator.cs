// Services/AspectCalculator.cs
using AstrologyChart.Models;
using System.Collections.Generic;
using System.Linq;

namespace AstrologyChart.Services
{
    public class AspectCalculator
    {
        public class AspectResult
        {
            public CelestialBody Body1 { get; set; }
            public CelestialBody Body2 { get; set; }
            public double Degree { get; set; }
            public double Deviation { get; set; }
            public bool IsApplying { get; set; }
        }

        public static List<AspectResult> CalculateAspects(List<CelestialBody> bodies, List<AspectSetting> settings)
        {
            var results = new List<AspectResult>();
            var pairs = bodies.SelectMany((x, i) => bodies.Skip(i + 1), (x, y) => (x, y));

            foreach (var (a, b) in pairs)
            {
                if (a.IsAngle && b.IsAngle) continue;//不输出四轴^四轴的相位
                foreach (var set in settings.OrderBy(s => s.PhaseType).ThenByDescending(s => s.Degree))
                {
                    double angle = CalculateAngle(a.Longitude, b.Longitude);
                    double deviation = Math.Abs(angle - set.Degree);

                    if (deviation <= set.Orb)
                    {
                        results.Add(new AspectResult
                        {
                            Body1 = a,
                            Body2 = b,
                            Degree = set.Degree,
                            Deviation = deviation,
                            IsApplying = a.Speed > b.Speed
                        });
                    }
                }
            }
            return results;
        }


        public static List<AspectResult> CalculateAspects(List<CelestialBody> bodies1, List<CelestialBody> bodies2, List<AspectSetting> settings)
        {
            var results = new List<AspectResult>();

            foreach (var a in bodies1)
            {
                foreach (var b in bodies2)
                {
                    if (a.IsAngle && b.IsAngle) continue; // 不输出四轴^四轴的相位
                    foreach (var set in settings.OrderBy(s => s.PhaseType).ThenByDescending(s => s.Degree))
                    {
                        double angle = CalculateAngle(a.Longitude, b.Longitude);
                        double deviation = Math.Abs(angle - set.Degree);

                        if (deviation <= set.Orb)
                        {
                            results.Add(new AspectResult
                            {
                                Body1 = a,
                                Body2 = b,
                                Degree = set.Degree,
                                Deviation = deviation,
                                IsApplying = a.Speed > b.Speed
                            });
                        }
                    }
                }
            }

            return results;
        }

        

        private static double CalculateAngle(double lon1, double lon2)
        {
            double angle = Math.Abs(lon1 - lon2) % 360;
            return angle > 180 ? 360 - angle : angle;
        }
    }
}