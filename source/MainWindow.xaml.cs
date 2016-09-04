using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace SotAMapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MapDataMgr _mapDataMgr;
        private PlayerDataWatcher _playerDataWatcher;

        private Map _lastMap;
        private PlayerData _lastPlayerData;

        public MainWindow()
        {
            InitializeComponent();

            // build window title, use embedded assembly version so it can be set
            // in one place.
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Title = "SotAMapper v" + ver.Major + "." + ver.Minor;

            RenderMap(null, null);

            // load up all available map files
            _mapDataMgr = new MapDataMgr();
            _mapDataMgr.Load();

            // start watching SotA log file for player /loc data
            _playerDataWatcher = new PlayerDataWatcher();
            _playerDataWatcher.PlayerDataChanged += OnPlayerDataChanged;
            _playerDataWatcher.Start();
        }

        /// <summary>
        /// Called whenever new player data is available.
        /// </summary>
        public void OnPlayerDataChanged(PlayerData newPlayerData)
        {
            //Debug.WriteLine(">>> player data changed: " + newPlayerData);

            // ensure current map matches current player data (may be null if there is no map
            // data for current player location)
            var currentMap = _mapDataMgr.GetMap(newPlayerData.MapName);

            // store for later use by size changed event handler
            _lastMap = currentMap;
            _lastPlayerData = newPlayerData;

            // trigger re-render on UI thread
            Dispatcher.BeginInvoke((Action)(() => RenderMap(currentMap, newPlayerData)));
        }

        /// <summary>
        /// When window size changes, need to re-render as everything is based on
        /// window size
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderMap(_lastMap, _lastPlayerData);
        }

        /// <summary>
        /// Render map data on WPF canvas
        /// </summary>
        private void RenderMap(Map map, PlayerData playerData)
        {
            double canvasX = 0, canvasY = 0;
            TextBlock label = null;

            // margin for edge text directly in canvas units
            double windowMargin = 5;

            // first, clear everything off the canvas
            MainCanvas.Children.Clear();

            // do we have valid data to render?

            bool validData = true;

            if ((map == null) || (playerData == null))
                validData = false;

            IEnumerable<MapCoord> otherData = null;
            if (playerData != null)
                otherData = new List<MapCoord> {playerData.Loc};
            var conv = new MapCanvasConverter(map, MainCanvas, otherData);
            if (!conv.Init())
                validData = false;

            // color/brush used for rendering text and other annotations
            var lineColor = Color.FromRgb(236, 142, 0);
            var lineBrush = new SolidColorBrush(lineColor);

            // no valid data, just show help text
            if (!validData)
            {
                var helpText =
                    "* * * NO DATA * * *\n\n" +
                    "Type /loc in game and verify reported map name matches a file in \"data/maps\"\n\n" +
                    "For example, if /loc outputs the below:\n" +
                    "Area: Soltown (Novia_R1_City_Soltown) Loc: (-15.7, 28.0, 23,2)\n" +
                    "there should be a file, \"data/maps/Novia_R1_City_Soltown.csv\" with map data\n\n" +
                    "Player location on map will update automatically, however it is necessary to\n" +
                    "manually use the /loc command once each time when entering a map to sync current map.\n\n" +                    
                    "Map items are rendered as text labels unless a PNG file exists in \"data/icons\" which\n" +
                    "matches the name of the item in the map .csv file\n\n" +
                    "The /loctrack command shows location on screen and makes it easier to build map files";
                label = new TextBlock();
                label.Text = helpText;
                label.Foreground = lineBrush;
                label.FontSize = 18;
                label.TextAlignment = TextAlignment.Center;             
                label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                label.Arrange(new Rect(label.DesiredSize));
                Canvas.SetLeft(label, (MainCanvas.ActualWidth / 2.0) - (label.ActualWidth / 2.0));
                Canvas.SetTop(label, conv.CanvasMarginY);
                MainCanvas.Children.Add(label);
            }

            // otherwise render map data
            else
            {
                // render all map items
                foreach (var mapItem in map.Items)
                {
                    conv.ConvertMapToCanvas(mapItem.Coord, out canvasX, out canvasY);

                    // if an icon exits for this item use that
                    var iconPath = System.IO.Path.Combine(Globals.IconDir, mapItem.Name + ".png");
                    if (File.Exists(iconPath))
                    {
                        var img = new Image();
                        var bmpImg = new BitmapImage();
                        bmpImg.BeginInit();
                        bmpImg.UriSource = new Uri(iconPath);
                        bmpImg.EndInit();
                        img.Stretch = Stretch.Fill;
                        img.Source = bmpImg;
                        img.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                        img.Arrange(new Rect(img.DesiredSize));
                        Canvas.SetLeft(img, canvasX - (img.ActualWidth / 2.0));
                        Canvas.SetTop(img, canvasY - (img.ActualHeight / 2.0));
                        MainCanvas.Children.Add(img);
                    }
                    
                    // otherwise, just use a text label
                    else
                    {
                        var myEll = new Ellipse();
                        myEll.Stroke = lineBrush;
                        myEll.Fill = lineBrush;
                        myEll.StrokeThickness = 2;
                        myEll.Width = myEll.Height = 10.0;
                        Canvas.SetLeft(myEll, canvasX - (myEll.Width / 2.0));
                        Canvas.SetTop(myEll, canvasY - (myEll.Height / 2.0));
                        MainCanvas.Children.Add(myEll);

                        label = new TextBlock();
                        label.Text = mapItem.Name;
                        label.Foreground = lineBrush;
                        label.FontSize = 18;
                        label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                        label.Arrange(new Rect(label.DesiredSize));
                        Canvas.SetLeft(label, canvasX - (label.ActualWidth / 2.0));
                        Canvas.SetTop(label, canvasY - (label.ActualHeight * 1.2));
                        MainCanvas.Children.Add(label);
                    }
                }

                conv.ConvertMapToCanvas(playerData.Loc, out canvasX, out canvasY);

                // render player using player icon if one exists
                var playerIconPath = System.IO.Path.Combine(Globals.IconDir, "Player.png");
                if (File.Exists(playerIconPath))
                {
                    var img = new Image();
                    var bmpImg = new BitmapImage();
                    bmpImg.BeginInit();
                    bmpImg.UriSource = new Uri(playerIconPath);
                    bmpImg.EndInit();
                    img.Stretch = Stretch.Fill;
                    img.Source = bmpImg;
                    img.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    img.Arrange(new Rect(img.DesiredSize));
                    Canvas.SetLeft(img, canvasX - (img.ActualWidth / 2.0));
                    Canvas.SetTop(img, canvasY - (img.ActualHeight / 2.0));
                    MainCanvas.Children.Add(img);
                }

                // otherwise render player using text
                else
                {
                    label = new TextBlock();
                    label.Text = "*";
                    label.Foreground = lineBrush;
                    label.FontSize = 40;
                    label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    label.Arrange(new Rect(label.DesiredSize));
                    Canvas.SetLeft(label, canvasX - (label.ActualWidth / 2.0));
                    Canvas.SetTop(label, canvasY - (label.ActualHeight / 2.0));
                    MainCanvas.Children.Add(label);
                }

                // render the compass image
                var compassIconPath = System.IO.Path.Combine(Globals.IconDir, "CompassDirections.png");
                if (File.Exists(compassIconPath))
                {
                    var img = new Image();
                    var bmpImg = new BitmapImage();
                    bmpImg.BeginInit();
                    bmpImg.UriSource = new Uri(compassIconPath);
                    bmpImg.EndInit();
                    img.Stretch = Stretch.Fill;
                    img.Source = bmpImg;
                    img.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    img.Arrange(new Rect(img.DesiredSize));
                    Canvas.SetLeft(img, conv.CanvasMarginX);
                    Canvas.SetTop(img, MainCanvas.ActualHeight - img.ActualHeight - conv.CanvasMarginY);
                    MainCanvas.Children.Add(img);
                }

                // show map name on top
                label = new TextBlock();
                label.Text = map.Name;
                label.Foreground = lineBrush;
                label.FontSize = 18;
                label.TextAlignment = TextAlignment.Center;
                label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                label.Arrange(new Rect(label.DesiredSize));
                Canvas.SetLeft(label, (MainCanvas.ActualWidth/2.0) - (label.ActualWidth/2.0));
                Canvas.SetTop(label, windowMargin);
                MainCanvas.Children.Add(label);
            }

            // show current time to indicate when map was last rendered
            var now = DateTime.Now;            
            label = new TextBlock();
            label.Text = "Map Rendered: " + now.ToString();
            label.Foreground = lineBrush;
            label.FontSize = 14;
            label.TextAlignment = TextAlignment.Right;
            label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            label.Arrange(new Rect(label.DesiredSize));
            Canvas.SetLeft(label, MainCanvas.ActualWidth - label.ActualWidth - windowMargin);
            Canvas.SetTop(label, MainCanvas.ActualHeight - label.ActualHeight - windowMargin);
            MainCanvas.Children.Add(label);
        }
    }
}


