// Models/CelestialBody.cs
using System;

namespace AstrologyChart.Models
{
    public class CelestialBody
    {
        public string Name { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public int House { get; set; }

        public bool IsAngle {  get; set; }
        public string ZodiacSign => GetZodiac(Longitude);
        public double ZodiacDegrees => Longitude % 30;

        private static string GetZodiac(double lon)//获取星座
        {
            string[] signs = { "白羊", "金牛", "双子", "巨蟹", "狮子", "处女",
                          "天秤", "天蝎", "射手", "摩羯", "水瓶", "双鱼" };
            int index = (int)(lon / 30);
            return signs[index];
        }
    }
}