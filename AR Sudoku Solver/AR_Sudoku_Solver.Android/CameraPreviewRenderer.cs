using System;
using System.IO;
using Android.Hardware;
using AR_Sudoku_Solver.Droid;
using AR_Sudoku_Solver;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using ApxLabs.FastAndroidCamera;
using SkiaSharp;
using OpenCvSharp;
using System.Threading.Tasks;

[assembly: ExportRenderer (typeof(AR_Sudoku_Solver.CameraPreview), typeof(CameraPreviewRenderer))]
namespace AR_Sudoku_Solver.Droid
{
	public class CameraPreviewRenderer : ViewRenderer<AR_Sudoku_Solver.CameraPreview, AR_Sudoku_Solver.Droid.CameraPreview>
	{
		CameraPreview cameraPreview;

		protected override void OnElementChanged (ElementChangedEventArgs<AR_Sudoku_Solver.CameraPreview> e)
		{
			base.OnElementChanged (e);

			if (Control == null) {
				cameraPreview = new CameraPreview (Context);
				SetNativeControl (cameraPreview);
			}

			if (e.OldElement != null) {
				// Unsubscribe
				cameraPreview.Click -= OnCameraPreviewClicked;
			}
			if (e.NewElement != null) {
				Control.Preview = Camera.Open ((int)e.NewElement.Camera);
                var parameters = Control.Preview.GetParameters();
                parameters.FocusMode = Camera.Parameters.FocusModeContinuousPicture;
                Control.Preview.SetParameters(parameters);

                int numBytes = (parameters.PreviewSize.Width * parameters.PreviewSize.Height * Android.Graphics.ImageFormat.GetBitsPerPixel(parameters.PreviewFormat)) / 8;
                using (FastJavaByteArray buffer = new FastJavaByteArray(numBytes))
                {
                    // allocate new Java byte arrays for Android to use for preview frames
                    Control.Preview.AddCallbackBuffer(new FastJavaByteArray(numBytes));
                }

                Control.Preview.SetNonMarshalingPreviewCallback(new AndroidImagePreviewCallbackHandler(e.NewElement.ImageHandlerCallback, cameraPreview));
				// Subscribe
				cameraPreview.Click += OnCameraPreviewClicked;
			}
		}

		void OnCameraPreviewClicked (object sender, EventArgs e)
		{
			if (cameraPreview.IsPreviewing) {
				cameraPreview.Preview.StopPreview ();
				cameraPreview.IsPreviewing = false;
			} else {
				cameraPreview.Preview.StartPreview ();
				cameraPreview.IsPreviewing = true;
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				Control.Preview.Release ();
			}
			base.Dispose (disposing);
		}
	}

    public class AndroidImagePreviewCallbackHandler : Java.Lang.Object, INonMarshalingPreviewCallback
    {
        public iImagePreviewHandler previewHandler;
        public bool isSizeSet = false;
        public int height = 1920;
        public int width = 1080;
        CameraPreview camerapreview;
        public SudokuFinder sf;
        public Mat gray;
        private bool runningLongProcessing = false;

        public AndroidImagePreviewCallbackHandler(iImagePreviewHandler previewHandler, CameraPreview camerapreview)
        {
            this.previewHandler = previewHandler;
            this.camerapreview = camerapreview;
            var assembly = typeof(CameraPreviewRenderer).Assembly;
            var stream = assembly.GetManifestResourceStream("AR_Sudoku_Solver.Droid.trainedKSVM.bin");
            
            byte[] trainedKSVM = new byte[stream.Length];
            for (int i =0; i < stream.Length; i++)
            {
                trainedKSVM[i] = (byte) stream.ReadByte();
            }
            sf = new SudokuFinder(trainedKSVM);
        }

        public unsafe void OnPreviewFrame(IntPtr data, Camera camera)
        {
            if (!isSizeSet) {
                height = camera.GetParameters().PreviewSize.Height;
                width = camera.GetParameters().PreviewSize.Width;
                StaticSKInfo.iheight = height;
                StaticSKInfo.iwidth = width;
                StaticSKInfo.sheight = camerapreview.surfaceView.Height;
                StaticSKInfo.swidth = camerapreview.surfaceView.Width;
                isSizeSet = true;
            }

            using (FastJavaByteArray buffer = new FastJavaByteArray(data))
            {
                StaticSKInfo.iheight = height;
                StaticSKInfo.iwidth = width;
                StaticSKInfo.sheight = camerapreview.surfaceView.Height;
                StaticSKInfo.swidth = camerapreview.surfaceView.Width;
                gray = new Mat(height, width, MatType.CV_8UC1, (IntPtr) buffer.Raw);
                if (sf.FindSudoku(gray))
                {
                    StaticSKInfo.bitmap.SetPixels(sf.unwarpedSudoku.Clone().DataStart);
                    StaticSKInfo.centres = sf.centres;
                    StaticSKInfo.corners = sf.corners;
                    StaticSKInfo.drawCorners = true;
                    if (!runningLongProcessing)
                    {
                        Task processingTask = new Task(LongImageProcessing);
                        processingTask.Start();
                    }
                }
                previewHandler.GotNewImageCB();
                camera.AddCallbackBuffer(buffer);           
            }
        }
        
        private void LongImageProcessing()
        {
            runningLongProcessing = true;
            var numbers = sf.ProcessPuzzle(sf.unwarpedSudoku);
            previewHandler.GotNewGrid(sf.OCR(numbers));
            runningLongProcessing = false;
        }
    }
}
