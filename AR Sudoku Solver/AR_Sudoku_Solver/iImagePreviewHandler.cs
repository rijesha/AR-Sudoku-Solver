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
        void GotNewGrid(Sudoku s);
    }
    
}
