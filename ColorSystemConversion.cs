using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reus2Surveyor
{
    internal class ColorSystemConversion
    {
        public static (float h, float s, float l) RgbToHsl(SixLabors.ImageSharp.Color color)
        {
            Rgba32 pix = color.ToPixel<Rgba32>();
            System.Drawing.Color sysColor = System.Drawing.Color.FromArgb(pix.A, pix.R, pix.G, pix.B);

            return (sysColor.GetHue(), sysColor.GetSaturation(), sysColor.GetBrightness());
        }

        public static SixLabors.ImageSharp.Color HslToRgb((float h, float s, float l) hsl)
        {
            float r = HslPiecewise(hsl, 0) * 255;
            float g = HslPiecewise(hsl, 8) * 255;
            float b = HslPiecewise(hsl, 4) * 255;
            return new(new Rgb24((byte)r, (byte)g, (byte)b));
        }

        internal static float HslPiecewise((float h, float s, float l) hsl, int n)
        {
            if (hsl.h < 0) hsl.h += 360;
            double k = (n + hsl.h / 30) % 12;
            double a = hsl.s * Math.Min(hsl.l, 1 - hsl.l);

            List<double> ks = [k - 3, 9 - k, 1];
            double f = hsl.l - a * Math.Max(-1, ks.Min());
            return (float)f;
        }
    }
}
