using Xamarin.Forms;
using System;
using SkiaSharp;

namespace AR_Sudoku_Solver
{
	public class CameraPreview : View
	{
        public static readonly BindableProperty CameraProperty = BindableProperty.Create (
			propertyName: "Camera",
			returnType: typeof(CameraOptions),
			declaringType: typeof(CameraPreview),
			defaultValue: CameraOptions.Rear);

		public CameraOptions Camera {
			get { return (CameraOptions)GetValue (CameraProperty); }
			set { SetValue (CameraProperty, value); }
		}

        public static readonly BindableProperty ImageHandlerCallbackProperty = BindableProperty.Create(
            propertyName: "ImageHandlerCallback",
            returnType: typeof(iImagePreviewHandler),
            declaringType: typeof(CameraPreview),
            defaultValue: new DummyImagePreviewHandler());

        public iImagePreviewHandler ImageHandlerCallback
        {
            get { return (iImagePreviewHandler)GetValue(ImageHandlerCallbackProperty); }
            set { SetValue(ImageHandlerCallbackProperty, value); }
        }
    }

    public class DummyImagePreviewHandler :  iImagePreviewHandler
    {
        public DummyImagePreviewHandler()
        {

        }

        public void GotNewImageCB()
        {
            System.Diagnostics.Debug.WriteLine("Got a new Image inside dummyImagePreviewHandler and doing nothing with it");
        }
    }
}
