using ImageMagick;
using PixelForge.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixelForge.Infrastructure.ImageProcessing
{
    public class ImageProccessingHelper
    {
        public MagickFormat ParseFormat(string format)
        {
            return format.ToLower() switch
            {
                "jpg" or "jpeg" => MagickFormat.Jpeg,
                "png" => MagickFormat.Png,
                "webp" => MagickFormat.WebP,
                "gif" => MagickFormat.Gif,
                _ => MagickFormat.Jpeg
            };
        }

        public Gravity GetGravity(WatermarkDirection direction)
        {
            return direction switch
            {
                WatermarkDirection.Top => Gravity.North,
                WatermarkDirection.Bottom => Gravity.South,
                WatermarkDirection.Left => Gravity.West,
                WatermarkDirection.Right => Gravity.East,
                WatermarkDirection.Center => Gravity.Center,
                WatermarkDirection.TopLeft => Gravity.Northwest,
                WatermarkDirection.TopRight => Gravity.Northeast,
                WatermarkDirection.BottomLeft => Gravity.Southwest,
                WatermarkDirection.BottomRight => Gravity.Southeast,
                _ => Gravity.Center
            };
        }
        public double GetRotation(WatermarkDirection direction)
        {
            return direction switch
            {
                WatermarkDirection.DiagonalLeft => 45,
                WatermarkDirection.DiagonalRight => -45,
                _ => 0
            };
        }
    }
}
