using System;
using System.Collections.Generic;
using System.IO;
using AstrologyChart.Models;
using SwissEphNet;

namespace AstrologyChart.Services
{
    public class AstroService
    {
        private readonly SwissEph _swiss;
        private const int FLAGS = SwissEph.SEFLG_SPEED;

        public AstroService()
        {
            _swiss = new SwissEph();
            _swiss.swe_set_ephe_path(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ephe"));
        }

        public List<CelestialBody> Calculate(DateTime utcTime, double longitude, double latitude, char houseSystem, int[] selectedBodies)
        {
            var bodies = new List<CelestialBody>();
            double julDay = _swiss.swe_julday(
                utcTime.Year, utcTime.Month, utcTime.Day,
                utcTime.TimeOfDay.TotalHours, SwissEph.SE_GREG_CAL);

            // 计算用户选择的行星和特殊点
            foreach (int id in selectedBodies)
            {
                double[] pos = new double[6];
                string err = "";
                _swiss.swe_calc_ut(julDay, id, FLAGS, pos, ref err);

                bodies.Add(new CelestialBody
                {
                    Name = GetPlanetName(id),
                    Longitude = pos[0],
                    Speed = pos[3]
                });
            }

            // 精确计算春分点
            var vernalPoint = _swiss.swe_get_ayanamsa(julDay) % 360;
            if (Array.IndexOf(selectedBodies, SwissEph.SE_ECL_NUT) != -1)
            {
                bodies.Add(new CelestialBody { Name = "春分点", Longitude = vernalPoint });
            }

            // 计算宫位系统
            double[] cusps = new double[14];
            double[] ascmc = new double[10];
            _swiss.swe_houses(julDay, latitude, longitude, houseSystem, cusps, ascmc);

            // 添加四轴
            bodies.AddRange(new[] {
                new CelestialBody { Name=" Asc ", Longitude=Mod360(ascmc[0]), IsAngle=true },
                new CelestialBody { Name=" MC ", Longitude=Mod360(ascmc[1]), IsAngle=true },
                new CelestialBody { Name=" Desc ", Longitude=Mod360(ascmc[0] + 180), IsAngle=true },
                new CelestialBody { Name=" IC ", Longitude=Mod360(ascmc[1] + 180), IsAngle=true }
            });

            // 计算宫位
            foreach (var body in bodies.Where(b => !b.IsAngle))
            {
                body.House = CalculateHouse(body.Longitude, cusps);
            }

            return bodies;
        }

        private int CalculateHouse(double bodyLon, double[] cusps)
        {
            bodyLon = Mod360(bodyLon);
            for (int i = 1; i <= 12; i++)
            {
                double start = Mod360(cusps[i]);
                double end = Mod360(cusps[i + 1]);
                if (start > end) end += 360;
                double bl = bodyLon < start ? bodyLon + 360 : bodyLon;

                if (bl >= start && bl < end)
                    return i;
            }
            return 1;
        }

        private double Mod360(double value) => (value % 360 + 360) % 360;


        // 见文档 https://www.astro.com/swisseph/swephprg.htm#_Toc112949032 ，3.2. Bodies (int ipl)

        private string GetPlanetName(int id) => id switch
        {
            SwissEph.SE_SUN => "太阳",
            SwissEph.SE_MOON => "月亮",
            SwissEph.SE_MERCURY => "水星",
            SwissEph.SE_VENUS => "金星",
            SwissEph.SE_MARS => "火星",
            SwissEph.SE_JUPITER => "木星",
            SwissEph.SE_SATURN => "土星",
            SwissEph.SE_URANUS => "天王星",
            SwissEph.SE_NEPTUNE => "海王星",
            SwissEph.SE_PLUTO => "冥王星",
            SwissEph.SE_MEAN_NODE => "北交点",
            SwissEph.SE_ECL_NUT => "春分点",
            _ => "未知"
        };
    }
}