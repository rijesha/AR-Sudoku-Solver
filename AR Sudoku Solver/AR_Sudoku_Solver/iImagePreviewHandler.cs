using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Sudoku_Solver
{
    public interface iImagePreviewHandler
    {
        void GotNewImageCB();
    }

    public interface iBufferedImage
    {
        IntPtr pointer { get; set; }
        int Count { get; set; }

        void DestroyBuffer();
    }
}
