using System;
using System.Collections.Generic;
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

            double mapWidth = 0;
            double mapHeight = 0;
            if (_map.MapCoordSys == CoordSysType.XZ_NorthWest)
            {
                mapWidth = (double)(maxZ - minZ);
                mapHeight = (double)(maxX - minX);
            }
            else if (_map.MapCoordSys == CoordSysType.ZX_NorthEast)
            {
                mapWidth = (double)(maxX - minX);
                mapHeight = (double)(maxZ - minZ);
            }
            else if (_map.MapCoordSys == CoordSysType.ZX_EastSouth)
            {
                mapWidth = (double)(maxZ - minZ);
                mapHeight = (double)(maxX - minX);
            }
            else if (_map.MapCoordSys == CoordSysType.ZX_SouthWest)
            {
                mapWidth = (double)(maxX - minX);
                mapHeight = (double)(maxZ - minZ);
            }
            else
            {
                return false;
            }

            // determine render size

            var canvasWidth = _canvas.ActualWidth;
            var canvasHeight = _canvas.ActualHeight;

            var renderWidth = canvasWidth - (CanvasMarginX * 2.0);
            var renderHeight = canvasHeight - (CanvasMarginY * 2.0);

            // compute scale factor from map to canvas in a way which
            // will preserve the aspect ratio and fit within the
            // avaiable space

            var widthScale = (mapWidth != 0) ? (renderWidth / mapWidth) : 0;
            var heightScale = (mapHeight != 0) ? (renderHeight / mapHeight) : 0;
            _mapToCanvasScale = Math.Min(widthScale, heightScale);

            // extreme corner of map data (north west corner) used
            // to translate the map to the canvas origin (upper left)

            if (_map.MapCoordSys == CoordSysType.XZ_NorthWest)
            {
                _mapUpperLeftX = (double)maxX.GetValueOrDefault();
                _mapUpperLeftZ = (double)maxZ.GetValueOrDefault();
            }
            else if (_map.MapCoordSys == CoordSysType.ZX_NorthEast)
            {
                _mapUpperLeftX = (double)minX.GetValueOrDefault();
                _mapUpperLeftZ = (double)maxZ.GetValueOrDefault();
            }
            else if (_map.MapCoordSys == CoordSysType.ZX_EastSouth)
            {
                _mapUpperLeftX = (double)minX.GetValueOrDefault();
                _mapUpperLeftZ = (double)minZ.GetValueOrDefault();
            }
            else if (_map.MapCoordSys == CoordSysType.ZX_SouthWest)
            {
                _mapUpperLeftX = (double)maxX.GetValueOrDefault();
                _mapUpperLeftZ = (double)minZ.GetValueOrDefault();
            }
            else
            {
                return false;
            }

            return true;
        }

        public void ConvertMapToCanvas(MapCoord mapLoc, out double canvasX, out double canvasY)
        {
            // special case, if map scale is zero, just map all coordinates to the center
            if (_mapToCanvasScale == 0)
            {
                canvasX = _canvas.ActualWidth / 2.0;
                canvasY = _canvas.ActualHeight / 2.0;
                return;
            }

            // scale map loc to canvas, reverse direction, and map Z=>X and X=>Y

            if (_map.MapCoordSys == CoordSysType.XZ_NorthWest)
            {
                canvasX = -(mapLoc.Z - _mapUpperLeftZ)*_mapToCanvasScale;
                canvasY = -(mapLoc.X - _mapUpperLeftX)*_mapToCanvasScale;
            }
            else if (_map.MapCoordSys == CoordSysType.ZX_NorthEast)
            {
                canvasX = (mapLoc.X - _mapUpperLeftX)*_mapToCanvasScale;
                canvasY = -(mapLoc.Z - _mapUpperLeftZ)*_mapToCanvasScale;
            }
            else if (_map.MapCoordSys == CoordSysType.ZX_EastSouth)
            {
                canvasX = (mapLoc.Z - _mapUpperLeftZ) * _mapToCanvasScale;
                canvasY = (mapLoc.X - _mapUpperLeftX) * _mapToCanvasScale;
            }
            else if (_map.MapCoordSys == CoordSysType.ZX_SouthWest)
            {
                canvasX = -(mapLoc.X - _mapUpperLeftX) * _mapToCanvasScale;
                canvasY = (mapLoc.Z - _mapUpperLeftZ) * _mapToCanvasScale;
            }
            else
            {
                canvasX = _canvas.ActualWidth / 2.0;
                canvasY = _canvas.ActualHeight / 2.0;
                return;
            }

            // add render margin offfset

            canvasX += CanvasMarginX;
            canvasY += CanvasMarginY;
        }
    }
}
