using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace AR_Sudoku_Solver
{
    public static class StaticSKInfo
    {
        public static SKPoint[] corners = null;
        public static SKPoint[] centres = null;
        public static SKBitmap bitmap = new SKBitmap(new SKImageInfo(432,432,SKColorType.Gray8));
        public static bool drawCorners = false;
        public static bool drawNumbers = false;
        public static int sheight = 100;
        public static int swidth = 100;
        public static int iheight = 100;
        public static int iwidth = 100;
        public static int[] numbers = new int[81];
        public static SKColor Color = SKColors.Orange;
    }
}
