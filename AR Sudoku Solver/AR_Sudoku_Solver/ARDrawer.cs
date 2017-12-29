using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Sudoku_Solver
{
    public class ARDrawer : iImagePreviewHandler
    {
        public SKCanvasView canvasview;
        public SKBitmap debugBitmap = new SKBitmap();
        public bool drawOuterBox = false;
        public SKPoint[] corners;
        public SKPoint[] centres;
        public int[] numbers;
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
            paint.Color = Xamarin.Forms.Color.Green.ToSKColor();
            paint.StrokeWidth = 15;
            paint.Style = SKPaintStyle.Stroke;
            canvas.Clear();
            if (!debugBitmap.IsNull)
            {
                canvas.DrawBitmap(debugBitmap, 0,0);
            }
            if (drawOuterBox)
            {
                var path = new SKPath();
                ConvertCoordinates(corners);
                path.AddPoly(corners);
                canvas.DrawPath(path, paint);
                drawOuterBox = false;

                ConvertCoordinates(centres);
                int i = 0;
                foreach (var c in centres)
                {
                    SKPaint textPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1,
                        FakeBoldText = true,
                        Color = StaticSKInfo.Color                        
                    };

                    textPaint.TextSize = 100f;
                    canvas.DrawPositionedText(numbers[i] == 0 ? "" : numbers[i].ToString(), new SKPoint[] { c }, textPaint);
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
            this.debugBitmap = StaticSKInfo.bitmap;
            this.corners = StaticSKInfo.corners;
            this.centres = StaticSKInfo.centres;
            this.drawOuterBox = StaticSKInfo.drawCorners;
            this.numbers = StaticSKInfo.numbers;
            canvasview.InvalidateSurface();
        }
    }
}
