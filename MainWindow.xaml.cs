using AstroTEXT.Models;
using AstroTEXT.Services;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AstrologyChart
{
    public partial class MainWindow : Window
    {
        private readonly AstroService _astroService = new();
        private ObservableCollection<AspectSetting> _aspectSettings = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadTimeZones();
            lstAspects.ItemsSource = _aspectSettings;
        }

        private void LoadTimeZones()
        {
            cmbTimeZone.ItemsSource = DateTimeZoneProviders.Tzdb.GetAllZones()
                .Where(z => z.Id.Contains("Asia") || z.Id.Contains("UTC"));
        }

        private void AddAspect_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtDegree.Text, out var deg) && double.TryParse(txtOrb.Text, out var orb))
            {
                _aspectSettings.Add(new AspectSetting { Degree = deg, Orb = orb });
            }
        }

        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            var utcTime = GetSelectedUtcTime();
            var bodies = await Task.Run(() => _astroService.Calculate(utcTime));
            var aspects = AspectCalculator.CalculateAspects(bodies, _aspectSettings.ToList());

            // 构建结果文本
            var sb = new StringBuilder();
            foreach (var body in bodies)
            {
                sb.AppendLine($"{body.Name}位于{body.ZodiacSign}座{body.ZodiacDegrees:F2}度，" +
                            $"黄经{body.Longitude:F2}；");
            }
            sb.AppendLine("\n相位关系：");
            aspects.ForEach(a => sb.AppendLine(a));

            txtResult.Text = sb.ToString();
            //Clipboard.SetText(txtResult.Text);
        }

        private DateTime GetSelectedUtcTime()
        {
            var localTime = dtPicker.SelectedDate ?? DateTime.Now;
            var tz = DateTimeZoneProviders.Tzdb[cmbTimeZone.SelectedValue?.ToString() ?? "UTC"];
            return Instant.FromDateTimeOffset(localTime).InZone(tz).ToDateTimeUtc();
        }
    }
}