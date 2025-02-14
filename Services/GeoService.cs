

// Services/GeoService.cs
using AstrologyChart.Models;
using System.Collections.Generic;

namespace AstrologyChart.Services
{
    public class GeoService
    {
        private readonly Dictionary<string, Dictionary<string, GeoData>> _geoDatabase = new()
        {
            ["中国"] = new() {
                { "北京", new GeoData(116.4074, 39.9042) },
                { "上海", new GeoData(121.4737, 31.2304) },
            },
            ["美国"] = new() {
                { "纽约", new GeoData(-74.0060, 40.7128) },
                { "洛杉矶", new GeoData(-118.2437, 34.0522) }
            }
        };

        public List<string> GetCountries() => new List<string>(_geoDatabase.Keys);

        public List<string> GetCities(string country) =>
            _geoDatabase.TryGetValue(country, out var cities) ? new List<string>(cities.Keys) : new List<string>();

        public GeoData? GetGeoData(string country, string city) =>
            _geoDatabase.TryGetValue(country, out var cities) && cities.TryGetValue(city, out var data) ? data : null;
    }
}