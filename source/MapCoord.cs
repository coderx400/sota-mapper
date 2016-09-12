using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SotAMapper
{
    /// <summary>
    /// SotA map coordinate
    /// </summary>
    public class MapCoord
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public MapCoord(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as MapCoord;
            if (rhs == null)
                return false;

            if ((rhs.X != X) ||
                (rhs.Y != Y) ||
                (rhs.Z != Z))
            {
                return false;
            }

            return true;
        }
    }
}
