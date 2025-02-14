// MainWindow.xaml.cs
using AstrologyChart.Models;
using AstrologyChart.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace AstrologyChart
{
    public partial class MainWindow : Window
    {
        private readonly GeoService _geoService = new();
        private readonly AstroService _astroService = new();
        private readonly ObservableCollection<AspectSetting> _aspectSettings = new();
        private readonly CollectionViewSource _aspectViewSource = new();

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimeComboBoxes(); // 初始化时间下拉选项
            InitializeHouseSystemComboBox(); // 初始化分宫制下拉菜单
            InitializeAspects();
            InitializeView();
        }

        private void InitializeView()
        {
            _aspectViewSource.Source = _aspectSettings;
            _aspectViewSource.View.SortDescriptions.Clear();
            _aspectViewSource.View.SortDescriptions.Add(new SortDescription("PhaseType", ListSortDirection.Ascending));
            _aspectViewSource.View.SortDescriptions.Add(new SortDescription("Degree", ListSortDirection.Descending));
            // 这里原代码中的 dgAspects 未使用到，可以考虑移除相关代码
            // dgAspects.ItemsSource = _aspectViewSource.View;
        }

        private void InitializeTimeComboBoxes()
        {
            // 初始化年份下拉框，范围从 1900 到 2100
            for (int year = 1900; year <= 2100; year++)
            {
                cmbYear.Items.Add(year);
            }
            cmbYear.SelectedItem = DateTime.Now.Year;

            // 初始化月份下拉框
            for (int month = 1; month <= 12; month++)
            {
                cmbMonth.Items.Add(month);
            }
            cmbMonth.SelectedItem = DateTime.Now.Month;

            // 初始化日期下拉框
            UpdateDayComboBox();

            // 初始化小时下拉框
            for (int hour = 0; hour < 24; hour++)
            {
                cmbHour.Items.Add(hour);
            }
            cmbHour.SelectedItem = DateTime.Now.Hour;

            // 初始化分钟下拉框
            for (int minute = 0; minute < 60; minute++)
            {
                cmbMinute.Items.Add(minute);
            }
            cmbMinute.SelectedItem = DateTime.Now.Minute;

            // 初始化秒下拉框
            for (int second = 0; second < 60; second++)
            {
                cmbSecond.Items.Add(second);
            }
            cmbSecond.SelectedItem = DateTime.Now.Second;
        }

        private void InitializeHouseSystemComboBox()
        {
            // 初始化宫制下拉菜单
            cmbHouseSystem.ItemsSource = new Dictionary<string, char> {
                {"整宫制 Whole", 'W'}, {"Koch", 'K'}, {"Placidus", 'P'}, {"Campanus", 'C'},
                {"Equal", 'E'}, {"Vehlow", 'V'}, {"Porphyrius", 'L'}, {"Regiomontanus", 'R'},
                {"Bianchini", 'B'}, {"Sunshine", 'U'}, {"Polich/Page", 'T'}, {"Carter", 'N'},
                {"McCla", 'M'}, {"Morinus", 'O'}, {"Hindu", 'I'}, {"Tropical", 'X'}
            }.Select(p => new { Display = p.Key, Value = p.Value });
            cmbHouseSystem.SelectedIndex = 0;
        }

        private void InitializeAspects()
        {
            AddPhaseGroup(PhaseType.Major, new[] { (0.0, 10.0), (180.0, 10.0), (120.0, 10.0), (90.0, 10.0) });
            AddPhaseGroup(PhaseType.Minor, new[] { (60.0, 5.0), (30.0, 5.0), (150.0, 5.0), (45.0, 5.0), (135.0, 5.0) });
        }

        private void AddPhaseGroup(PhaseType type, IEnumerable<(double deg, double orb)> aspects)
        {
            foreach (var (deg, orb) in aspects)
            {
                if (!_aspectSettings.Any(a => a.Degree == deg))
                {
                    _aspectSettings.Add(new AspectSetting { Degree = deg, Orb = orb, PhaseType = type });
                }
            }
        }

        #region 输入验证
        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(newText, @"^\d{0,8}$");
        }

        private void DatePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void DoubleValidation(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^-?\d*\.?\d*$");
            e.Handled = !regex.IsMatch(((TextBox)e.Source).Text + e.Text);
        }
        #endregion

        #region 事件处理
        //private void OnCountryChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (cmbCountry.SelectedItem is string country)
        //    {
        //        cmbCity.ItemsSource = _geoService.GetCities(country);
        //        cmbCity.SelectedIndex = 0;
        //    }
        //}

        //private void OnCityChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (cmbCountry.SelectedItem is string country && cmbCity.SelectedItem is string city)
        //    {
        //        var data = _geoService.GetGeoData(country, city);
        //        if (data != null)
        //        {
        //            txtLongitude.Text = data.Longitude.ToString("F5");
        //            txtLatitude.Text = data.Latitude.ToString("F5");
        //        }
        //    }
        //}

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var utcTime = GetUtcTime();
            var houseSystem = ((dynamic)cmbHouseSystem.SelectedItem).Value; // 获取分宫制字符
            var selectedBodies =lstBodies.SelectedItems.Cast<ListBoxItem>().Select(item => int.Parse(item.Tag.ToString())).ToArray();

            var bodies = _astroService.Calculate(
                utcTime,
                double.Parse(txtLongitude.Text),
                double.Parse(txtLatitude.Text),
                houseSystem,
                selectedBodies);

            txtResult.Text = GenerateReport(bodies);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtResult.Text);
            MessageBox.Show("已复制到剪贴板");
        }

        private void TimeCombo_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (comboBox.Items.Count == 0) return;

            int currentIndex = comboBox.SelectedIndex;
            if (e.Delta > 0)
            {
                // 向上滚动，索引减 1
                if (currentIndex > 0)
                {
                    comboBox.SelectedIndex = currentIndex - 1;
                }
            }
            else
            {
                // 向下滚动，索引加 1
                if (currentIndex < comboBox.Items.Count - 1)
                {
                    comboBox.SelectedIndex = currentIndex + 1;
                }
            }

            if (comboBox == cmbMonth)
            {
                UpdateDayComboBox();
            }
        }

        private void UpdateDayComboBox()
        {
            cmbDay.Items.Clear();
            if (cmbYear.SelectedItem != null && cmbMonth.SelectedItem != null)
            {
                int year = (int)cmbYear.SelectedItem;
                int month = (int)cmbMonth.SelectedItem;
                int daysInMonth = DateTime.DaysInMonth(year, month);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    cmbDay.Items.Add(day);
                }
                cmbDay.SelectedItem = Math.Min((int?)cmbDay.SelectedItem ?? 1, daysInMonth);
            }
        }
        #endregion

        #region 辅助方法
        private DateTime GetUtcTime()
        {
            if (cmbYear.SelectedItem != null && cmbMonth.SelectedItem != null && cmbDay.SelectedItem != null &&
                cmbHour.SelectedItem != null && cmbMinute.SelectedItem != null && cmbSecond.SelectedItem != null)
            {
                int year = (int)cmbYear.SelectedItem;
                int month = (int)cmbMonth.SelectedItem;
                int day = (int)cmbDay.SelectedItem;
                int hour = (int)cmbHour.SelectedItem;
                int minute = (int)cmbMinute.SelectedItem;
                int second = (int)cmbSecond.SelectedItem;
                return new DateTime(year, month, day, hour, minute, second).ToUniversalTime();
            }
            return DateTime.UtcNow;
        }

        private bool ValidateInputs()
        {
            if (cmbYear.SelectedItem == null || cmbMonth.SelectedItem == null || cmbDay.SelectedItem == null ||
                cmbHour.SelectedItem == null || cmbMinute.SelectedItem == null || cmbSecond.SelectedItem == null)
            {
                MessageBox.Show("请选择完整的时间信息");
                return false;
            }

            if (!double.TryParse(txtLongitude.Text, out var lng) || Math.Abs(lng) > 180)
            {
                MessageBox.Show("经度需在-180到180之间");
                return false;
            }

            if (!double.TryParse(txtLatitude.Text, out var lat) || Math.Abs(lat) > 90)
            {
                MessageBox.Show("纬度需在-90到90之间");
                return false;
            }

            return true;
        }
        #endregion

        private string GenerateReport(List<CelestialBody> bodies)
        {
            var aspects = AspectCalculator.CalculateAspects(bodies, _aspectSettings.ToList());
            var sb = new StringBuilder();

            sb.AppendLine("天体位置：");
            foreach (var body in bodies.OrderBy(b => b.House))// 按宫排序
            {
                sb.AppendLine($"{body.Name}位于{body.ZodiacSign}座{body.ZodiacDegrees:F2}度{(body.IsAngle ? "" : $"第{body.House}宫")};");
            }

            var aspectGroups = aspects
               .GroupBy(a => a.Degree)
               .OrderBy(g => g.Key)
               .Select(g => new {
                   Degree = g.Key,
                   Aspects = g.OrderBy(a => a.Deviation).ToList()
               });
            sb.AppendLine("\n相位关系：");
            foreach (var group in aspectGroups)
            {
                sb.AppendLine($"【{group.Degree}°相位】");
                foreach (var aspect in group.Aspects)
                {
                    string applying = aspect.IsApplying ? "入相位" : "出相位";
                    var body1 = aspect.Body1.Name;
                    sb.AppendLine($"{(aspect.Body1.IsAngle ? "" : $"第{aspect.Body1.House}宫的")}{aspect.Body1.Name}与{(aspect.Body2.IsAngle ? "" : $"第{aspect.Body2.House}宫的")}{aspect.Body2.Name}形成{group.Degree}°相位（误差{aspect.Deviation:F2}度，{applying}）");
                }
            }
            return sb.ToString();
        }
    }
}