﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ViewSupport
{
    // TODO:P1 Currently, Negative Coords are emitted as-is.  Should intentionally clip or offset/shift negative xy coords before conversion.
    public class SvgSerializer
    {
        public SvgSerializer()
        {

        }

        public List<ArcShape> DeSerializeJson(string filePath)
        {
            string arcSegsJsonText = File.ReadAllText(filePath);
            object parsedObj = JsonConvert.DeserializeObject(arcSegsJsonText, typeof(List<ArcShape>));
            List<ArcShape> arcSegs = parsedObj as List<ArcShape>;

            return arcSegs;
        }


        public StringBuilder Serialize(List<ArcShape> arcSegs, List<VectorShape> vectorShapes)
        {
            // SVG Format:
            // - https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorial/Paths
            // - Absolute Uppercase, Relative Lowercase
            //   L x y  Line to x y coord
            //   H x    Horizontal Line
            //   V y    Vertical Line
            //   M x1 y1    Move to...
            //   A rx ry ang large-arc-flag[0|1] sweep-flag[0|1] X2 Y2

            float maxX = 1;
            float maxY = 1;

            CultureInfo culture = new CultureInfo("en-US");
            StringBuilder sbArcsPath = new StringBuilder();
            StringBuilder sbArcs = new StringBuilder();

            if (arcSegs != null)
            {
                float pi = (float)Math.PI;

                arcSegs.Sort((as1, as2) =>
                    {
                        string as1Hash =
                            $"{as1.EdgeID:000000}:{Math.Round(as1.ZeroCoord.X, 2):000000}:{Math.Round(as1.ZeroCoord.Y, 2):000000}:{as1.StartAngle}:{as1.SweepAngle}";
                        string as2Hash =
                            $"{as2.EdgeID:000000}:{Math.Round(as2.ZeroCoord.X, 2):000000}:{Math.Round(as2.ZeroCoord.Y, 2):000000}:{as2.StartAngle}:{as2.SweepAngle}";

                        return as1Hash.CompareTo(as2Hash);
                    });

                // svg arc: https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorial/Paths#arcs
                foreach (var arcSeg in arcSegs)
                {
                    // TODO:P1 Should 0 and/or small angle sweep be skipped or merged with close by sweeps?  Maybe 0 or small angle gaps are logic/rounding BUG needing to be fixed?

                    // Massage sweep to minimum of 1 degree.
                    Debug.Assert(arcSeg.SweepAngle >= 0);
                    int sweepAngle = Math.Max(1, arcSeg.SweepAngle);

                    int sweepFlag = 1;
                    float startRad = (arcSeg.StartAngle * pi) / 180;
                    float endRad = ((arcSeg.StartAngle + sweepAngle) * pi) / 180;
                    float r = arcSeg.ArcRect.Width / 2;

                    float cx = arcSeg.ArcRect.X + arcSeg.ArcRect.Width / 2;
                    float cy = arcSeg.ArcRect.Y + arcSeg.ArcRect.Height / 2;
                    float x1 = cx + (float)(r * Math.Cos(startRad));
                    float y1 = cy + (float)(r * Math.Sin(startRad));
                    float x2 = cx + (float)(r * Math.Cos(endRad));
                    float y2 = cy + (float)(r * Math.Sin(endRad));

                    if (x1 > maxX) maxX = x1;
                    if (x2 > maxX) maxX = x2;
                    if (y1 > maxY) maxY = y1;
                    if (y2 > maxY) maxY = y2;

                    sbArcsPath.AppendLine(string.Format(culture, "M {0} {1}", x1, y1));
                    sbArcsPath.AppendLine(string.Format(culture, "A {0} {0} 0 0 {1} {2} {3}", r, sweepFlag, x2, y2));
                }

                sbArcs.Append("<path stroke=\"black\" stroke-width=\"1\" d=\"");
                sbArcs.AppendLine(sbArcsPath.ToString());
                sbArcs.Append("\"/>");
            }

            StringBuilder sbVectors = new StringBuilder();
            if (vectorShapes != null)
            {
                foreach (var shape in vectorShapes)
                {
                    LineShape line = shape as LineShape;
                    if (line != null)
                    {
                        float x1 = line.Start.X;
                        float y1 = line.Start.Y;
                        float x2 = line.End.X;
                        float y2 = line.End.Y;

                        if (x1 > maxX) maxX = x1;
                        if (x2 > maxX) maxX = x2;
                        if (y1 > maxY) maxY = y1;
                        if (y2 > maxY) maxY = y2;

                        StringBuilder sbVectorPath = new StringBuilder();
                        sbVectorPath.AppendLine(string.Format(culture, "M {0} {1}", x1, y1));
                        sbVectorPath.AppendLine(string.Format(culture, "L {0} {1}", x2, y2));

                        sbVectors.Append($"<path stroke=\"{line.Color}\" stroke-width=\"1\" d=\"");
                        sbVectors.AppendLine(sbVectorPath.ToString());
                        sbVectors.Append("\"/>");
                        sbVectorPath.Clear();
                    }

                    PathShape pathShape = shape as PathShape;
                    if (pathShape != null)
                    {
                        StringBuilder sbVectorPath = new StringBuilder();
                        foreach (var pathEdge in pathShape.Path)
                        {
                            float x1 = pathEdge.Item1.X;
                            float y1 = pathEdge.Item1.Y;
                            float x2 = pathEdge.Item2.X;
                            float y2 = pathEdge.Item2.Y;

                            if (x1 > maxX) maxX = x1;
                            if (x2 > maxX) maxX = x2;
                            if (y1 > maxY) maxY = y1;
                            if (y2 > maxY) maxY = y2;

                            sbVectorPath.AppendLine(string.Format(culture, "M {0} {1}", x1, y1));
                            sbVectorPath.AppendLine(string.Format(culture, "L {0} {1}", x2, y2));
                        }

                        sbVectors.Append($"<path stroke=\"{pathShape.Color}\" stroke-width=\"1\" d=\"");
                        sbVectors.AppendLine(sbVectorPath.ToString());
                        sbVectors.Append("\"/>");
                        sbVectorPath.Clear();
                    }
                }

            }

            int width = (int)Math.Ceiling(maxX);
            int height = (int)Math.Ceiling(maxY);

            StringBuilder sbDoc = new StringBuilder();
            sbDoc.AppendLine(string.Format(culture, "<svg width=\"{0}\" height=\"{1}\" viewBox=\"0 0 {0} {1}\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\">", width, height));

            sbDoc.AppendLine(sbArcs.ToString());

            sbDoc.AppendLine(sbVectors.ToString());

            sbDoc.AppendLine("</svg>");

            return sbDoc;
        }
    }
}
