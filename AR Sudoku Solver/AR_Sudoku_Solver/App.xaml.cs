using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace AR_Sudoku_Solver
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            MainPage = new AR_Sudoku_Solver.MainPage();
            SKCanvasView canvasview = MainPage.FindByName<SKCanvasView>("canvasview");
            ARDrawer imageprocessor = new ARDrawer(canvasview);

            CameraPreview campreview = MainPage.FindByName<CameraPreview>("CameraPreview");
            campreview.ImageHandlerCallback = imageprocessor;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
