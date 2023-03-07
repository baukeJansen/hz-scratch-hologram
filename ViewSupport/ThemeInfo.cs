﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ViewSupport
{
    public class ThemeInfo
    {
        static ThemeInfo()
        {
        }

        public static ThemeInfo LightTheme = new ThemeInfo()
        {
            WindowBackColor = System.Drawing.SystemColors.Control,
            TextColor = Color.FromArgb(0, 0, 0),
            BackgroundColor = Color.White,
            BorderColor = System.Drawing.SystemColors.InactiveBorder,
            ArcPen = new Pen(Color.Gray),
            ArcPenHighlight = new Pen(Color.LimeGreen, 1f),
            ArcTextBrush = Brushes.DarkGray,
            VectorPen = new Pen(Color.Blue),
            ExportVectorColor = "Black",
            PointBrush = Brushes.Blue,
            RedBlueModePointBrush = Brushes.Black,  // Can't be Blue
            SelectedColor = Color.Red,
            SelectedPen = new Pen(Color.Red, 1f)
        };

        public static ThemeInfo DarkTheme = new ThemeInfo()
        {
            WindowBackColor = Color.FromArgb(45, 45, 48), //Color.FromArgb(96, 96, 96), // Goal is Color.FromArgb(45, 45, 48) after UI fully updated
            TextColor = Color.FromArgb(241, 241, 241),
            BackgroundColor = Color.FromArgb(30, 30, 30),
            BorderColor = Color.FromArgb(67, 67, 70),
            ArcPen = new Pen(Color.DarkGray),
            ArcPenHighlight = new Pen(Color.FromArgb(96, 128, 255)),  // new Pen(Color.LimeGreen, 1f),
            ArcTextBrush = Brushes.LightGray,
            VectorPen = new Pen(Color.LightBlue),
            ExportVectorColor = "Black",
            PointBrush = Brushes.LightGray,
            RedBlueModePointBrush = Brushes.White,
            SelectedColor = Color.MediumPurple,
            SelectedPen = new Pen(Color.Red, 2.5f)
        };

        public static ThemeInfo Current = DarkTheme;

        public Color WindowBackColor { get; set; }
        public Color TextColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color BorderColor { get; set; }
        public Pen ArcPen { get; set; }
        public Pen ArcPenHighlight { get; set; }
        public Pen VectorPen { get; set; }
        
        public string ExportVectorColor { get; set; }

        public Brush PointBrush { get; set; }

        public Brush RedBlueModePointBrush { get; set; }

        public Brush ArcTextBrush { get; set; }

        public Color SelectedColor { get; set; }

        public Pen SelectedPen { get; set; }
    }
}
