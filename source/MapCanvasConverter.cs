using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SotAMapper
{
    /// <summary>
    /// convert between SotA map coords and (WPF drawing) canvas coords so that
    /// all data in the map scales into available rendering area on the canvas
    /// while preserving the aspect ratio of the data.
    /// 
    /// SotA Coords:  X,Y,Z => (North+,Up+,West+)
    /// Canvas Coord: X,Y => (Right+,Down+)
    /// </summary>
    public class MapCanvasConverter
    {
        private Map _map;
        private Canvas _canvas;
        private IEnumerable<MapCoord> _otherData;

        // margin maintained on the edge of the canvas so renderirng doesn't
        // occur right up to the edge.  specified as a % of the total width/height
        // of the drawing canvas
        private const double _canvasMarginWidthPercent = 5;
        private const double _canvasMarginHeightPercent = 5;

        // computed canvas margin values
        public double CanvasMarginX { get; private set; }
        public double CanvasMarginY { get; private set; }

        private double _mapUpperLeftX;
        private double _mapUpperLeftZ;

        private double _mapToCanvasScale;

        /// <summary>
        /// Converter needs the map data to be rendered, the canvas on which the
        /// data will be rendered, and any additional other map-like data to be rendered.
        /// "other" data would be, for example, the player position.
        /// </summary>
        public MapCanvasConverter(Map map, Canvas canvas, IEnumerable<MapCoord> otherData)
        {
            _map = map;
            _canvas = canvas;
            _otherData = otherData;

            // compute margin values, needed regardless of whether Init() succeeds

            var canvasWidth = _canvas.ActualWidth;
            var canvasHeight = _canvas.ActualHeight;

            CanvasMarginX = canvasWidth * (_canvasMarginWidthPercent / 100.0);
            CanvasMarginY = canvasHeight * (_canvasMarginHeightPercent / 100.0);
        }

        public bool Init()
        {
            if ((_map == null) || (_canvas == null) || (_map.MinLoc == null) || (_map.MaxLoc == null))
                return false;

            // determine extents of map data

            float? minX = _map.MinLoc.X;
            float? minZ = _map.MinLoc.Z;
            float? maxX = _map.MaxLoc.X;
            float? maxZ = _map.MaxLoc.Z;

            if (_otherData != null)
            {
                foreach (var otherDataCoord in _otherData)
                {
                    Utils.CheckAndSetMin(ref minX, otherDataCoord.X);
                    Utils.CheckAndSetMin(ref minZ, otherDataCoord.Z);
                    Utils.CheckAndSetMax(ref maxX, otherDataCoord.X);
                    Utils.CheckAndSetMax(ref maxZ, otherDataCoord.Z);
                }
            }

            double mapWidth = (double)(maxZ - minZ);
            double mapHeight = (double)(maxX - minX);

            if ((mapWidth == 0) || (mapHeight == 0))
                return false;

            // determine render size

            var canvasWidth = _canvas.ActualWidth;
            var canvasHeight = _canvas.ActualHeight;

            var renderWidth = canvasWidth - (CanvasMarginX * 2.0);
            var renderHeight = canvasHeight - (CanvasMarginY * 2.0);

            // compute scale factor from map to canvas in a way which
            // will preserve the aspect ratio and fit within the
            // avaiable space

            var widthScale = renderWidth / mapWidth;
            var heightScale = renderHeight / mapHeight;
            _mapToCanvasScale = Math.Min(widthScale, heightScale);

            // extreme corner of map data (north west corner) used
            // to translate the map to the canvas origin (upper left)

            _mapUpperLeftX = (double)maxX.GetValueOrDefault();
            _mapUpperLeftZ = (double)maxZ.GetValueOrDefault();

            return true;
        }

        public void ConvertMapToCanvas(MapCoord mapLoc, out double canvasX, out double canvasY)
        {
            // scale map loc to canvas, reverse direction, and map Z=>X and X=>Y

            canvasX = -(mapLoc.Z - _mapUpperLeftZ) * _mapToCanvasScale;
            canvasY = -(mapLoc.X - _mapUpperLeftX) * _mapToCanvasScale;

            // add render margin offfset

            canvasX += CanvasMarginX;
            canvasY += CanvasMarginY;
        }
    }
}
