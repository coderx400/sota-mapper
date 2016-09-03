using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SotAMapper
{
    public static class Utils
    {
        public static void CheckAndSetMin(ref float? minVal, float val)
        {
            if ((minVal == null) ||
                (val < minVal))
            {
                minVal = val;
            }
        }

        public static void CheckAndSetMax(ref float? maxVal, float val)
        {
            if ((maxVal == null) ||
                (val > maxVal))
            {
                maxVal = val;
            }
        }
    }
}
