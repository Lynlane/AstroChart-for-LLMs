using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroTEXT.Models;
using SwissEphNet;
using System.IO;


namespace AstroTEXT.Services
{
    public class AstroService
    {
        private readonly SwissEph _swiss;
        private const int FLAGS = SwissEph.SEFLG_SPEED;

        public AstroService()
        {
            _swiss = new SwissEph();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ephe");
            /*  sepl_18.se1
                semo_18.se1
                sase_18.se1 */
            _swiss.swe_set_ephe_path(path);
        }

        public List<CelestialBody> Calculate(DateTime utcTime)
        {
            var bodies = new List<CelestialBody>();
            double julDay = _swiss.swe_julday(
                utcTime.Year, utcTime.Month, utcTime.Day,
                utcTime.TimeOfDay.TotalHours, SwissEph.SE_GREG_CAL);

            // 计算主要天体 (日、月、水...冥王)
            int[] planetIds = { SwissEph.SE_SUN, SwissEph.SE_MOON, SwissEph.SE_MERCURY,
                          SwissEph.SE_VENUS, SwissEph.SE_MARS, SwissEph.SE_JUPITER,
                          SwissEph.SE_SATURN, SwissEph.SE_URANUS, SwissEph.SE_NEPTUNE,
                          SwissEph.SE_PLUTO };

            foreach (int id in planetIds)
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
            return bodies;
        }

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
            _ => "未知"
        };
    }
}
