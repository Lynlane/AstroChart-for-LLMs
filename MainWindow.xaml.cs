using AstrologyChart.Models;
using AstrologyChart.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static AstrologyChart.Services.AspectCalculator;

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
            InitializeTimeComboBoxes(); // 时间
            InitializeTimeZoneComboBox(); // 时区
            InitializeHouseSystemComboBox(); // 分宫制
            InitializeAspects();
            InitializeView();
            chkOutputInterChartAspects.IsEnabled = grpChart2.Visibility == Visibility.Visible;//复选框
        }

        private void InitializeView()
        {
            _aspectViewSource.Source = _aspectSettings;
            _aspectViewSource.View.SortDescriptions.Clear();
            _aspectViewSource.View.SortDescriptions.Add(new SortDescription("PhaseType", ListSortDirection.Ascending));
            _aspectViewSource.View.SortDescriptions.Add(new SortDescription("Degree", ListSortDirection.Descending));

            dgAspects.ItemsSource = _aspectViewSource.View;
        }
        #region 初始化
        private void InitializeTimeComboBoxes()// 时间
        {
            // 初始化年份下拉框，范围从 1900 到 2100
            for (int year = 1900; year <= 2100; year++)
            {
                cmbYear.Items.Add(year);
                cmbYear2.Items.Add(year);
            }
            cmbYear.SelectedItem = DateTime.Now.Year;
            cmbYear2.SelectedItem = DateTime.Now.Year;

            // 初始化月份下拉框
            for (int month = 1; month <= 12; month++)
            {
                cmbMonth.Items.Add(month);
                cmbMonth2.Items.Add(month);
            }
            cmbMonth.SelectedItem = DateTime.Now.Month;
            cmbMonth2.SelectedItem = DateTime.Now.Month;

            // 初始化日期下拉框
            UpdateDayComboBox(cmbYear, cmbMonth, cmbDay);
            UpdateDayComboBox(cmbYear2, cmbMonth2, cmbDay2);

            // 初始化小时下拉框
            for (int hour = 0; hour < 24; hour++)
            {
                cmbHour.Items.Add(hour);
                cmbHour2.Items.Add(hour);
            }
            cmbHour.SelectedItem = DateTime.Now.Hour;
            cmbHour2.SelectedItem = DateTime.Now.Hour;

            // 初始化分钟下拉框
            for (int minute = 0; minute < 60; minute++)
            {
                cmbMinute.Items.Add(minute);
                cmbMinute2.Items.Add(minute);
            }
            cmbMinute.SelectedItem = DateTime.Now.Minute;
            cmbMinute2.SelectedItem = DateTime.Now.Minute;

            // 初始化秒下拉框
            for (int second = 0; second < 60; second++)
            {
                cmbSecond.Items.Add(second);
                cmbSecond2.Items.Add(second);
            }
            cmbSecond.SelectedItem = DateTime.Now.Second;
            cmbSecond2.SelectedItem = DateTime.Now.Second;
        }
        #endregion

        private void InitializeTimeZoneComboBox()
        {
            // 获取所有可用的时区
            var timeZones = TimeZoneInfo.GetSystemTimeZones();
            cmbTimeZone.ItemsSource = timeZones;
            cmbTimeZone.DisplayMemberPath = "DisplayName";
            // 默认选择系统当前时区
            cmbTimeZone.SelectedItem = TimeZoneInfo.Local;

            cmbTimeZone2.ItemsSource = timeZones;
            cmbTimeZone2.DisplayMemberPath = "DisplayName";
            cmbTimeZone2.SelectedItem = TimeZoneInfo.Local;
        }

        private void InitializeHouseSystemComboBox()
        {
            // 初始化宫制下拉菜单
            cmbHouseSystem.ItemsSource = new Dictionary<string, char> {
                {"Whole", 'W'}, {"Koch", 'K'}, {"Placidus", 'P'}, {"Campanus", 'C'},
                {"Equal", 'E'}, {"Vehlow", 'V'}, {"Porphyrius", 'L'}, {"Regiomontanus", 'R'},
                {"Bianchini", 'B'}, {"Sunshine", 'U'}, {"Polich/Page", 'T'}, {"Carter", 'N'},
                {"McCla", 'M'}, {"Morinus", 'O'}, {"Hindu", 'I'}, {"Tropical", 'X'}
            }.Select(p => new { Display = p.Key, Value = p.Value });
            cmbHouseSystem.SelectedIndex = 0;// 默认整宫制
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


        private void AddChart2_Click(object sender, RoutedEventArgs e)
        {
            grpChart2.Visibility = Visibility.Visible;
            btnAddChart2.Visibility = Visibility.Collapsed;
            btnRemoveChart2.Visibility = Visibility.Visible;
            chkOutputInterChartAspects.IsEnabled = true;// 复选框
        }

        private void RemoveChart2_Click(object sender, RoutedEventArgs e)
        {
            grpChart2.Visibility = Visibility.Collapsed;
            btnAddChart2.Visibility = Visibility.Visible;
            btnRemoveChart2.Visibility = Visibility.Collapsed;
            chkOutputInterChartAspects.IsEnabled = false;// 复选框
        }

        string chart1Name;
        string chart2Name;
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var utcTime1 = GetUtcTime(cmbYear, cmbMonth, cmbDay, cmbHour, cmbMinute, cmbSecond, cmbTimeZone);
            var houseSystem = ((dynamic)cmbHouseSystem.SelectedItem).Value; // 获取分宫制字符
            var selectedBodies = lstBodies.SelectedItems.Cast<ListBoxItem>().Select(item => int.Parse(item.Tag.ToString())).ToArray();
            bool isTropical = cmbZodiacSystem.SelectedIndex == 0;
            var enabledAspects = _aspectSettings.Where(a => a.IsEnabled);

            var bodies1 = _astroService.Calculate(
                utcTime1,
                double.Parse(txtLongitude.Text),
                double.Parse(txtLatitude.Text),
                houseSystem,
                selectedBodies,
                isTropical
            );

            chart1Name = string.IsNullOrEmpty(txtChart1Name.Text) ? "星图1" : txtChart1Name.Text;
            var sb = new StringBuilder();
            sb.AppendLine($"【{chart1Name}】");
            sb.AppendLine(GenerateReport(bodies1));//打印星图1信息

            if (grpChart2.Visibility == Visibility.Visible)
            {
                var utcTime2 = GetUtcTime(cmbYear2, cmbMonth2, cmbDay2, cmbHour2, cmbMinute2, cmbSecond2, cmbTimeZone2);
                var bodies2 = _astroService.Calculate(
                    utcTime2,
                    double.Parse(txtLongitude2.Text),
                    double.Parse(txtLatitude2.Text),
                    houseSystem,
                    selectedBodies,
                    isTropical
                );

                chart2Name = string.IsNullOrEmpty(txtChart2Name.Text) ? "星图2" : txtChart2Name.Text;
                sb.AppendLine($"【{chart2Name}】");
                sb.AppendLine(GenerateReport(bodies2));//打印星图2信息

                if (chkOutputInterChartAspects.IsChecked == true)
                {
                    var interChartAspects = AspectCalculator.CalculateAspects(bodies1, bodies2, enabledAspects.ToList());
                    sb.AppendLine("\n【两星图之间的相位关系】");


                    /*手动写个排序……lamba表达式排序总是报错，不太会用*/

                    // 存储分组结果的字典，键是 Degree，值是该 Degree 对应的 AspectResult 列表
                    Dictionary<double, List<AspectResult>> groupedAspects = new Dictionary<double, List<AspectResult>>();

                    // 手动进行分组操作
                    foreach (var aspect in interChartAspects)
                    {
                        if (!groupedAspects.ContainsKey(aspect.Degree))
                        {
                            groupedAspects[aspect.Degree] = new List<AspectResult>();
                        }
                        groupedAspects[aspect.Degree].Add(aspect);
                    }

                    // 存储最终排序后的分组结果的列表
                    List<(double Degree, List<AspectResult> Aspects)> aspectGroups = new List<(double Degree, List<AspectResult> Aspects)>();

                    // 对分组结果按 Degree 进行排序
                    var sortedKeys = new List<double>(groupedAspects.Keys);
                    sortedKeys.Sort();

                    foreach (var key in sortedKeys)
                    {
                        // 对每个分组内的 AspectResult 按 Deviation 进行排序
                        var sortedGroup = groupedAspects[key];
                        sortedGroup.Sort((a, b) => a.Deviation.CompareTo(b.Deviation));

                        aspectGroups.Add((key, sortedGroup));
                    }



                    foreach (var group in aspectGroups)
                    {
                        sb.AppendLine($"【{group.Degree}°相位】");
                        foreach (var aspect in group.Aspects)
                        {
                            string applying = aspect.IsApplying ? "入相位" : "出相位";
                            sb.AppendLine($"{chart1Name}{aspect.Body1.ZodiacSign}座{(aspect.Body1.IsAngle ? "" : $"第{aspect.Body1.House}宫的")}{aspect.Body1.Name}与{chart2Name}{aspect.Body2.ZodiacSign}座{(aspect.Body2.IsAngle ? "" : $"第{aspect.Body2.House}宫的")}{aspect.Body2.Name}呈{group.Degree}°相位（误差{aspect.Deviation:F2}度，{applying}）");
                        }
                    }
                }
            }

            txtResult.Text = sb.ToString();
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
                UpdateDayComboBox(cmbYear, cmbMonth, cmbDay);
            }
            else if (comboBox == cmbMonth2)
            {
                UpdateDayComboBox(cmbYear2, cmbMonth2, cmbDay2);
            }
        }

        private void UpdateDayComboBox(ComboBox yearComboBox, ComboBox monthComboBox, ComboBox dayComboBox)
        {
            dayComboBox.Items.Clear();
            if (yearComboBox.SelectedItem != null && monthComboBox.SelectedItem != null)
            {
                int year = (int)yearComboBox.SelectedItem;
                int month = (int)monthComboBox.SelectedItem;
                int daysInMonth = DateTime.DaysInMonth(year, month);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    dayComboBox.Items.Add(day);
                }
                dayComboBox.SelectedItem = Math.Min((int?)dayComboBox.SelectedItem ?? 1, daysInMonth);
            }
        }
        #endregion

        #region 辅助方法
        private DateTime GetUtcTime(ComboBox yearComboBox, ComboBox monthComboBox, ComboBox dayComboBox, ComboBox hourComboBox, ComboBox minuteComboBox, ComboBox secondComboBox, ComboBox timeZoneComboBox)
        {
            if (yearComboBox.SelectedItem != null && monthComboBox.SelectedItem != null && dayComboBox.SelectedItem != null &&
                hourComboBox.SelectedItem != null && minuteComboBox.SelectedItem != null && secondComboBox.SelectedItem != null)
            {
                int year = (int)yearComboBox.SelectedItem;
                int month = (int)monthComboBox.SelectedItem;
                int day = (int)dayComboBox.SelectedItem;
                int hour = (int)hourComboBox.SelectedItem;
                int minute = (int)minuteComboBox.SelectedItem;
                int second = (int)secondComboBox.SelectedItem;
                var localTime = new DateTime(year, month, day, hour, minute, second);
                var timeZone = (TimeZoneInfo)timeZoneComboBox.SelectedItem;
                return TimeZoneInfo.ConvertTimeToUtc(localTime, timeZone);
            }
            return DateTime.UtcNow;
        }

        private bool ValidateInputs()
        {
            if (cmbYear.SelectedItem == null || cmbMonth.SelectedItem == null || cmbDay.SelectedItem == null ||
                cmbHour.SelectedItem == null || cmbMinute.SelectedItem == null || cmbSecond.SelectedItem == null)
            {
                MessageBox.Show("请选择完整的时间信息（星图 1）");
                return false;
            }

            if (!double.TryParse(txtLongitude.Text, out var lng1) || Math.Abs(lng1) > 180)
            {
                MessageBox.Show("星图 1 经度需在 -180 到 180 之间");
                return false;
            }

            if (!double.TryParse(txtLatitude.Text, out var lat1) || Math.Abs(lat1) > 90)
            {
                MessageBox.Show("星图 1 纬度需在 -90 到 90 之间");
                return false;
            }

            if (grpChart2.Visibility == Visibility.Visible)
            {
                if (cmbYear2.SelectedItem == null || cmbMonth2.SelectedItem == null || cmbDay2.SelectedItem == null ||
                    cmbHour2.SelectedItem == null || cmbMinute2.SelectedItem == null || cmbSecond2.SelectedItem == null)
                {
                    MessageBox.Show("请选择完整的时间信息（星图 2）");
                    return false;
                }

                if (!double.TryParse(txtLongitude2.Text, out var lng2) || Math.Abs(lng2) > 180)
                {
                    MessageBox.Show("星图 2 经度需在 -180 到 180 之间");
                    return false;
                }

                if (!double.TryParse(txtLatitude2.Text, out var lat2) || Math.Abs(lat2) > 90)
                {
                    MessageBox.Show("星图 2 纬度需在 -90 到 90 之间");
                    return false;
                }
            }

            return true;
        }
        #endregion

        private string GenerateReport(List<CelestialBody> bodies)
        {
            var enabledAspects = _aspectSettings.Where(a => a.IsEnabled);
            var aspects = AspectCalculator.CalculateAspects(bodies, enabledAspects.ToList());
            var sb = new StringBuilder();

            sb.AppendLine("天体位置：");
            foreach (var body in bodies.OrderBy(b => b.House))// 按宫排序
            {
                sb.AppendLine($"{body.Name}位于{body.ZodiacSign}座{body.ZodiacDegrees:F2}度{(body.IsAngle ? "" : $"第{body.House}宫")};");
            }

            var aspectGroups = aspects
               .GroupBy(a => a.Degree)
               .OrderBy(g => g.Key)
               .Select(g => new
               {
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
                    sb.AppendLine($"{aspect.Body1.ZodiacSign}座{(aspect.Body1.IsAngle ? "" : $"第{aspect.Body1.House}宫的")}{aspect.Body1.Name}与{aspect.Body2.ZodiacSign}座{(aspect.Body2.IsAngle ? "" : $"第{aspect.Body2.House}宫的")}{aspect.Body2.Name}呈{group.Degree}°相位（误差{aspect.Deviation:F2}度，{applying}）");
                }
            }

            return sb.ToString();
        }
    }
}