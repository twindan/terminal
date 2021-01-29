//
// Copyright (C) Microsoft.  All rights reserved.
// Licensed under the terms described in the LICENSE file in the root of this project.
//

using System;
using System.Drawing;
using System.Linq;

namespace ColorTool
{
    public static class ColorUtility
    {
        public static Color UIntToColor(uint color)
        {
            byte r = (byte)(color >> 0);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 16);
            return Color.FromArgb(r, g, b);
        }

    }
}
