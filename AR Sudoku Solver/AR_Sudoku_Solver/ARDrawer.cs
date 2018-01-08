using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AR_Sudoku_Solver
{
    public class ARDrawer : iImagePreviewHandler
    {
        public SKCanvasView canvasview;
        public bool newnumbers = false;

        public ARDrawer(SKCanvasView canvasview)
        {
            this.canvasview = canvasview;
            canvasview.PaintSurface += PaintCanvas;
        }

        private void PaintCanvas(object sender, SKPaintSurfaceEventArgs e)
        {
            int surfaceWidth = e.Info.Width;
            int surfaceHeight = e.Info.Height;
            SKCanvas canvas = e.Surface.Canvas;
            SKPaint paint = new SKPaint();
            paint.Color = SKColors.Black;
            paint.StrokeWidth = 15;
            paint.Style = SKPaintStyle.Stroke;
            canvas.Clear();

            if (StaticSKInfo.drawCorners)
            {
                var path = new SKPath();
                ConvertCoordinates(StaticSKInfo.corners);
                path.AddPoly(StaticSKInfo.corners);
                canvas.DrawPath(path, paint);
                StaticSKInfo.drawCorners = false;

                ConvertCoordinates(StaticSKInfo.centres);
                int i = 0;
                foreach (var c in StaticSKInfo.centres)
                {
                    SKPaint textPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1,
                        FakeBoldText = true,
                        Color = SKColors.Black                     
                    };

                    SKPaint textPaint2 = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1,
                        FakeBoldText = true,
                        Color = SKColors.DarkGreen
                    };

                    textPaint.TextSize = 100f;
                    textPaint2.TextSize = 100f;
                    if (StaticSKInfo.solvedNumbers[i] != 0)
                    {
                        canvas.DrawPositionedText(StaticSKInfo.solvedNumbers[i].ToString(), new SKPoint[] { c }, textPaint2);
                    }
                    canvas.DrawPositionedText(StaticSKInfo.detectedNumbers[i] == 0 ? "" : StaticSKInfo.detectedNumbers[i].ToString(), new SKPoint[] { c }, textPaint);
                    i++;
                }
            }
        }

        public void ConvertCentres(SKPoint[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new SKPoint(StaticSKInfo.swidth * points[i].Y / StaticSKInfo.iheight, StaticSKInfo.sheight * points[i].X / StaticSKInfo.iwidth);
            }
        }

        public void ConvertCoordinates(SKPoint[] points)
        {
            for (int i =0; i < points.Length; i++)
            {
                points[i] = new SKPoint(StaticSKInfo.swidth - StaticSKInfo.swidth * points[i].Y / StaticSKInfo.iheight, StaticSKInfo.sheight * points[i].X / StaticSKInfo.iwidth);
            }
        }

        public void GotNewImageCB()
        {
            canvasview.InvalidateSurface();
        }

        private List<Sudoku> sudokuList = new List<Sudoku>();
        CancellationTokenSource cts = new CancellationTokenSource();
        int currentlyProcessingHash = 0;

        public void GotNewGrid(Sudoku s)
        {
            s = s.GetSimilarSudoku(sudokuList);
            StaticSKInfo.detectedNumbers = s.detectedNumbers;
            if (currentlyProcessingHash != s.GetHashCode() || s.puzzleUpdated)
            {
                s.preparePuzzle();
                currentlyProcessingHash = s.GetHashCode();
                cts.Cancel();
                cts = new CancellationTokenSource();
                Task.Run(() => s.solvePuzzle(cts.Token));
            }
        }

    }
}
