using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reus2Surveyor
{
    internal class TernaryTileHeatmap
    {
        // Axes convention:
        // Triangle has one corner directly up
        // A-B-C clockwise from top corner

        // In direction of A, int step goes from [0..n-1] in n steps

        public readonly int Steps;
        public readonly int Points;
        public readonly List<(double a, double b, double c)> PercentPoints;
        public readonly List<(int a, int b, int c)> TilePoints;
        public Dictionary<(int a, int b, int c), int> TileCounts { get; private set; } = [];
        public Dictionary<(int a, int b, int c), double> TilePercents { get; private set; } = [];

        private void InitializeTiles()
        {
            this.TileCounts = [];
            this.TilePercents = [];
            int limit = this.Steps;
            for (int a = 0; a < limit; a++)
            {
                for (int b = 0; b < (limit - a); b++)
                {
                    int c = limit - a - b - 1;
                    this.TileCounts[(a, b, c)] = 0;
                }
            }
            limit = this.Steps - 1;
            for (int a = 0; a < limit; a++)
            {
                for (int b = 0; b < (limit - a); b++)
                {
                    int c = limit - a - b - 1;
                    this.TileCounts[(a, b, c)] = 0;
                }
            }
        }

        public TernaryTileHeatmap(int steps, List<(double, double, double)> data)
        {
            this.Steps = steps;
            this.Points = data.Count;
            this.InitializeTiles();

            this.PercentPoints = [.. data.Select(s => Normalize3Tuple(s))];
            this.TilePoints = [.. this.PercentPoints.Select(p => PercentToTile(p, this.Steps))];
            foreach ((int a, int b, int c) p in this.TilePoints)
            {
                this.TileCounts[p]++;
            }
            this.TilePercents = this.TileCounts.ToDictionary(kv => kv.Key, kv => (double)kv.Value / (double)this.Points);
        }

        public static FontCollection drawingFontCollection = new();
        public static FontFamily drawingFont = drawingFontCollection.Add("Font/NotoSans-VariableFont_wdth,wght.ttf");

        public Image DrawStandardPlot(Color bg, Func<double, double, (int a, int b, int c), Color> shader,
            string title, string labelA = "A", string labelB = "B", string labelC = "A", float blurMult = 1f
            )
        {
            int width = 500;
            int height = 450;
            Image image = new Image<Rgba32>(width, height, bg);
            PointF plotOrigin = new(70, 400);
            int sideLength = 360;

            // Draw smaller triangles
            double pMax = this.TilePercents.Values.Max();
            foreach ((int a, int b, int c) t in this.TileCounts.Keys)
            {
                PointF vA, vB, vC;
                if (t.a + t.b + t.c == this.Steps - 1)
                {
                    vA = TernaryTileToPointF((t.a + 1, t.b, t.c), sideLength, this.Steps) + plotOrigin;
                    vB = TernaryTileToPointF((t.a, t.b + 1, t.c), sideLength, this.Steps) + plotOrigin;
                    vC = TernaryTileToPointF((t.a, t.b, t.c + 1), sideLength, this.Steps) + plotOrigin;
                }
                else if (t.a + t.b + t.c == this.Steps - 2)
                {
                    vA = TernaryTileToPointF((t.a, t.b + 1, t.c + 1), sideLength, this.Steps) + plotOrigin;
                    vB = TernaryTileToPointF((t.a + 1, t.b, t.c + 1), sideLength, this.Steps) + plotOrigin;
                    vC = TernaryTileToPointF((t.a + 1, t.b + 1, t.c), sideLength, this.Steps) + plotOrigin;
                }
                else
                {
                    throw new Exception("Something messed up finding the vertices of the triangle.");
                }
                Polygon subTriangle = new([vA, vB, vC]);
                Color fillColor = shader(this.TilePercents[t], pMax, t);
                FillPolygon(ref image, subTriangle, fillColor);
            }

            // Gaussian blur the triangle
            // Then draw outline of plot
            PointF plotVertA = TernaryTileToPointF((this.Steps, 0, 0), sideLength, this.Steps) + plotOrigin;
            PointF plotVertB = TernaryTileToPointF((0, this.Steps, 0), sideLength, this.Steps) + plotOrigin;
            Polygon mainTriangle = new([plotVertA, plotVertB, plotOrigin]);
            if (blurMult > 0)
            {
                using (Image clone = image.Clone(x => x.GaussianBlur(blurMult * sideLength / this.Steps / 3)))
                {
                    clone.Mutate(x => x.Crop((Rectangle)mainTriangle.Bounds));
                    ImageBrush cloneBrush = new ImageBrush(clone);
                    image.Mutate(x => x.Fill(cloneBrush, mainTriangle));
                }
            }
            FillAndOutlinePolygon(ref image, mainTriangle, Color.Transparent, Color.Black, 3);

            // Titles and labels
            Font fontTitle = drawingFont.CreateFont(32, FontStyle.Bold);
            Font fontLabel = drawingFont.CreateFont(24, FontStyle.Regular);
            image.Mutate(x => x.DrawText(title, fontTitle, Color.Black, new PointF(8,0)));
            image.Mutate(x => x.DrawText(labelA, fontLabel, Color.Black, new Point(280, 84)));
            image.Mutate(x => x.DrawText(labelB, fontLabel, Color.Black, new Point(400, 408)));
            image.Mutate(x => x.DrawText(labelC, fontLabel, Color.Black, new Point(20, 408)));

            // Outer border
            FillAndOutlinePolygon(ref image, new Rectangle(0, 0, width, height), Color.Transparent, Color.Black, 5);

            return image;
        }

        public static Color SimpleShaderBase(double p, double pMax, (int a, int b, int c) t, Color stopColor)
        {
            return stopColor.WithAlpha((float)(p / pMax));
        }

        public static Func<double, double, (int a, int b, int c), Color> MakeSimpleShader(Color stopColor)
        {
            Func<double, double, (int a, int b, int c), Color> f = (double p, double pMax, (int a, int b, int c) s) => SimpleShaderBase(p, pMax, s, stopColor);
            return f;
        }

        public static Color CompositionShaderBase(double p, double pMax, (int a, int b, int c) t, Color colorA, Color colorB, Color colorC)
        {
            (double a, double b, double c) tn = Normalize3Tuple(((double)t.a, (double)t.b, (double)t.c));
            (float h, float s, float l) hslA = ColorSystemConversion.RgbToHsl(colorA);
            (float h, float s, float l) hslB = ColorSystemConversion.RgbToHsl(colorB);
            (float h, float s, float l) hslC = ColorSystemConversion.RgbToHsl(colorC);

            // Mix S and L simply
            float finalS = (float)(hslA.s * tn.a + hslB.s * tn.b + hslC.s * tn.c);
            float finalL = (float)(hslA.l * tn.a + hslB.l * tn.b + hslC.l * tn.c);

            // Mix H as a vector
            double hx = tn.a * Math.Cos(hslA.h * Math.PI / 180) + tn.b * Math.Cos(hslB.h * Math.PI / 180) + tn.c * Math.Cos(hslC.h * Math.PI / 180);
            double hy = tn.a * Math.Sin(hslA.h * Math.PI / 180) + tn.b * Math.Sin(hslB.h * Math.PI / 180) + tn.c * Math.Sin(hslC.h * Math.PI / 180);
            if (hx == 0 && hy == 0) return ColorSystemConversion.HslToRgb((0, 0, finalL));
            float finalH = (float)(Math.Atan2(hy, hx) * 180 / Math.PI);
            if (finalH < 0) finalH += 360;

            Color finalColor = ColorSystemConversion.HslToRgb((finalH, finalS, finalL));
            (float h, float s, float l) hslCheck = ColorSystemConversion.RgbToHsl(finalColor);
            return finalColor.WithAlpha((float)(p / pMax));
        }

        public static Func<double, double, (int a, int b, int c), Color> MakeCompositionShader(Color colorA, Color colorB, Color colorC)
        {
            Func<double, double, (int a, int b, int c), Color> f = (double p, double pMax, (int a, int b, int c) s) => CompositionShaderBase(p, pMax, s, colorA, colorB, colorC);
            return f;
        }

        internal static void FillAndOutlinePolygon(ref Image image, IPath polygon, Color fillColor, Color outlineColor, float outlineWidth)
        {
            Brush fillBrush = new SolidBrush(fillColor);
            Pen outlinePen = new SolidPen(outlineColor, outlineWidth);
            image.Mutate(x => x.Fill(fillBrush, polygon).Draw(outlinePen, polygon));
        }

        internal static void FillAndOutlinePolygon(ref Image image, Rectangle rectangle, Color fillColor, Color outlineColor, float outlineWidth)
        {
            Brush fillBrush = new SolidBrush(fillColor);
            Pen outlinePen = new SolidPen(outlineColor, outlineWidth);
            image.Mutate(x => x.Fill(fillBrush, rectangle).Draw(outlinePen, rectangle));
        }

        internal static void FillPolygon(ref Image image, IPath polygon, Color fillColor)
        {
            Brush fillBrush = new SolidBrush(fillColor);
            image.Mutate(x => x.Fill(fillBrush, polygon));
        }

        internal static (double a, double b, double c) Normalize3Tuple((double a, double b, double c) r)
        {
            if (r.a < 0 || r.b < 0 || r.c < 0) throw new ArgumentException($"Negative value in tuple ({r.a},{r.b},{r.c})");
            double total = r.a + r.b + r.c;
            if (total == 0) throw new ArgumentException($"Tuple ({r.a},{r.b},{r.c}) sums to zero");
            double a, b, c;
            a = r.a / total; b = r.b / total; c = r.c / total;
            return (a, b, c);
        }

        internal static (int a, int b, int c) PercentToTile((double a, double b, double c) p, int steps)
        {
            int a = (int)(p.a * steps);
            int b = (int)(p.b * steps);
            int c = (int)(p.c * steps);
            if ((a + b + c) >= steps)
            {
                // at the corner between 2 divisions (including at vertex of plot)
                // Nudge it towards the center, such that the shift will not take it more than 1 step

                double na = ((steps * p.a) + (1.0 / 3.0)) / (double)(1 + steps);
                double nb = ((steps * p.b) + (1.0 / 3.0)) / (double)(1 + steps);
                double nc = ((steps * p.c) + (1.0 / 3.0)) / (double)(1 + steps);
                return PercentToTile((na, nb, nc), steps);
            }
            else return (a, b, c);
        }

        public static TernaryTileHeatmap TestPercentToTile()
        {
            return new TernaryTileHeatmap(5,
                [
                (1,1,1), // 1,1,1: center
                (0.97, 0.01, 0.02), // 4,0,0: near vertex
                (0.45, 0.35, 0.30), // 2,1,1: upright triangle
                (0.3, 0.11, 0.59), // 1,0,2: inverted triangle

                // On 1 boundary, does not require shift towards center
                (0.7, 0, 0.3), // 3,0,1: edge of plot, on 1 boundary
                (0.6, 0.1, 0.3), // 3,0,1: internal edge, on 1 boundary

                // On 2 boundaries, requires shift towards center
                (1,0,0), // 5,0,0 -> 4,0,0: plot vertex
                (0.8, 0, 0.2), // 4,0,1 -> 3,0,1: corner on edge of plot
                (0.6, 0.2, 0.2), // 3,1,1 -> 2,1,1: internal corner

                ]);
        }

        // Origin of the ternary plot is (a,b,c) = 0,0,1
        // X-axis goes RIGHT
        // Y axis goes DOWN
        // Angles are clockwise starting from X axis
        /// <summary>
        /// Calculates the cartesian coordinates from percentages on a ternary plot
        /// <para />Origin of the ternary plot is (a,b,c) = (0,0,1), X-axis goes RIGHT, Y axis goes DOWN, Angles are clockwise starting from X axis
        /// </summary>
        /// <param name="p">Percentages as a 3-tuple of doubles. Will be normalized.</param>
        /// <param name="sideLength">Side length of the ternary plot</param>
        /// <param name="degA">Angle of the A-axis from the X-axis in degrees. Default 60 degrees, A is on top of the upright triangle.</param>
        /// <param name="degB">Angle of the B-axis from the X-axis in degrees. Default 0 degrees, B is to the right.</param>
        /// <returns></returns>
        public static (double x, double y) TernaryCompositionToCartesian((double a, double b, double c) p, double sideLength, double degA = -60, double degB = 0)
        {
            (double a, double b, double c) pn = Normalize3Tuple(p);
            double radA = degA * Math.PI / 180;
            double radB = degB * Math.PI / 180;

            double x = (pn.a * Math.Cos(radA)) + (pn.b * Math.Cos(radB));
            x *= sideLength;
            double y = (pn.a * Math.Sin(radA)) + (pn.b * Math.Sin(radB));
            y *= sideLength;
            return (x, y);
        }

        public static SixLabors.ImageSharp.PointF TernaryCompositionToPointF((double a, double b, double c) p, double sideLength, double degA = -60, double degB = 0)
        {
            (double x, double y) xy = TernaryCompositionToCartesian(p, sideLength, degA, degB);
            return new((float)xy.x, (float)xy.y);
        }

        public static (double x, double y) TernaryTileToCartesian((int a, int b, int c) t, double sideLength, int steps, double degA = -60, double degB = 0)
        {
            double radA = degA * Math.PI / 180;
            double radB = degB * Math.PI / 180;

            double ap = sideLength * t.a / steps;
            double bp = sideLength * t.b / steps;

            double x = (ap * Math.Cos(radA)) + (bp * Math.Cos(radB));
            double y = (ap * Math.Sin(radA)) + (bp * Math.Sin(radB));
            return (x, y);
        }

        public static SixLabors.ImageSharp.PointF TernaryTileToPointF((int a, int b, int c) t, double sideLength, int steps, double degA = -60, double degB = 0)
        {
            (double x, double y) xy = TernaryTileToCartesian(t, sideLength, steps, degA, degB);
            return new((float)xy.x, (float)xy.y);
        }
    }
}
