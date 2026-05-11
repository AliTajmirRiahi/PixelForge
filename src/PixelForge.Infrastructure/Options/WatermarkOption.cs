using System;
using System.Collections.Generic;
using System.Text;

namespace PixelForge.Infrastructure.Options
{
    public enum WatermarkDirection
    {
        Top,
        Bottom,
        Left,
        Right,
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        DiagonalLeft,   // ↘
        DiagonalRight,   // ↙
        TiledDiagonal,
    }
    public class WatermarkOption
    {
        public bool IsActive { get; set; }

        public required string Mark { get; set; }

        public WatermarkDirection Direction { get; set; }

        public required string Color { get; set; }

        public int Opacity { get; set; } = 100;
    }
}
