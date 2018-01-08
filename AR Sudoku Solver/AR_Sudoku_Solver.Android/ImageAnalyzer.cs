using System;
using System.Collections.Generic;
using OpenCvSharp;
using Accord.Statistics.Kernels;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.MachineLearning.VectorMachines;
using System.Runtime.InteropServices;
using SkiaSharp;
using System.Linq;

namespace AR_Sudoku_Solver.Droid
{
    public class SudokuFinder {

        Mat gray = new Mat();
        public Mat unwarpedSudoku;
        private Mat kernelHorz = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(27, 1));
        private Mat kernelVert = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, 27));
        private Mat horzSubtract = new Mat();
        private Mat vertSubtract = new Mat();
        private Point[] likelyCandidate;
        public MulticlassSupportVectorMachine<IKernel> saveKern;

        Point2f[] unwarpedcentres = new Point2f[81];
        public SKPoint[] corners = new SKPoint[4];
        public SKPoint[] centres = new SKPoint[81];
        Point[][] contours;
        HierarchyIndex[] hierarchyIndex;

        public SudokuFinder(byte[] savedKern = null, string savedKernPath = null) {
            if (savedKern!= null)
            {
                Accord.IO.Serializer.Load(savedKern, out saveKern);
            }
            else if (savedKernPath != null)
            {
                Accord.IO.Serializer.Load(savedKernPath, out saveKern);
            }
            unwarpedSudoku = new Mat(432, 432, MatType.CV_8UC1);
            int boxsize = 432 / 9;
            int ox = boxsize * 3/4, oy = ox, ind = 0;
            for (int j = 0; j < 9; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    var t = new Mat(3, 1, MatType.CV_64FC1);
                    t.Set(0, ox + i * boxsize);
                    t.Set(1, oy + j * boxsize);
                    t.Set(2, 1);
                    unwarpedcentres[ind] = new Point2f(ox + i * boxsize, oy + j * boxsize);
                    ind++;
                }
            }
        }
        
        public bool FindSudoku(Mat gray)
        {
            this.gray = gray;
            Cv2.AdaptiveThreshold(gray, gray, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 101, 5);
            Cv2.BitwiseNot(gray, gray);

            Cv2.FindContours(gray, out contours, out hierarchyIndex, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            if (!findContourOfPuzzle())
                return false;

            for (int i = 0; i < 4; i++)
            {
                corners[i] = new SKPoint(likelyCandidate[i].X, likelyCandidate[i].Y);
            }

            unwarpSudokuSquare();
            return true;
        }

        public List<Mat> ProcessPuzzle(Mat unwarpedPuzzle)
        {
            Cv2.Erode(unwarpedPuzzle, horzSubtract, kernelHorz);
            Cv2.Dilate(horzSubtract, horzSubtract, kernelHorz);

            Cv2.Erode(unwarpedPuzzle, vertSubtract, kernelVert);
            Cv2.Dilate(vertSubtract, vertSubtract, kernelVert);

            unwarpedPuzzle = unwarpedPuzzle - (horzSubtract + vertSubtract);
            
            return getIndividualBoxes(unwarpedPuzzle);
        }

        public Mat ConcatPuzzle(List<Mat> data)
        {
            Mat output = new Mat();
            Mat r1 = new Mat(), r2 = new Mat(), r3 = new Mat(), r4 = new Mat(), r5 = new Mat(), r6 = new Mat(), r7 = new Mat(), r8 = new Mat(), r9 = new Mat();
            Cv2.HConcat(data.Take(9).ToArray(), r1);
            Cv2.HConcat(data.Skip(9).Take(9).ToArray(), r2);
            Cv2.HConcat(data.Skip(18).Take(9).ToArray(), r3);
            Cv2.HConcat(data.Skip(27).Take(9).ToArray(), r4);
            Cv2.HConcat(data.Skip(36).Take(9).ToArray(), r5);
            Cv2.HConcat(data.Skip(45).Take(9).ToArray(), r6);
            Cv2.HConcat(data.Skip(54).Take(9).ToArray(), r7);
            Cv2.HConcat(data.Skip(63).Take(9).ToArray(), r8);
            Cv2.HConcat(data.Skip(72).Take(9).ToArray(), r9);

            Cv2.VConcat(new Mat[] { r1, r2, r3, r4, r5, r6, r7, r8, r9 }, output);
            return output;
        }

        private List<Mat> getIndividualBoxes(Mat unwarped)
        {
            int pixelnum = 3;
            List<Mat> boxes = new List<Mat>();
            Point[][] contours;
            HierarchyIndex[] hierarchyIndex;
            int inc = unwarped.Rows / 9;
            for (int x = 0; x < 9; x += 1)
            {
                for (int y = 0; y < 9; y += 1)
                {
                    int xind = x * inc;
                    int yind = y * inc;
                    var num = unwarped.SubMat(xind + pixelnum, xind - pixelnum + inc, yind + pixelnum, yind - pixelnum + inc);
                    if (Cv2.CountNonZero(num) < 100)
                    {
                        boxes.Add(null);
                    }
                    else
                    {
                        num.FindContours(out contours, out hierarchyIndex, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                        Point[] LargestContour = new Point[1];
                        double maxArea = 0;
                        foreach (var c in contours)
                        {
                            double tempArea = Cv2.ContourArea(c);
                            if (tempArea > maxArea)
                            {
                                maxArea = tempArea;
                                LargestContour = c;
                            }
                        }
                        int w = 25;
                        Rect rec = Cv2.BoundingRect(LargestContour);
                        Mat numbox = new Mat(w, w, MatType.CV_8UC1);

                        var dest = new Point2f[] { new Point2f(0, 0), new Point2f(0, w), new Point2f(w, w), new Point2f(w, 0) };

                        var transform = Cv2.GetPerspectiveTransform(rect2Contour(rec), dest);
                        Cv2.WarpPerspective(num, numbox, transform, new Size(w, w));

                        boxes.Add(numbox);
                    }
                }
            }
            return boxes;
        }

        private List<Point2f> rect2Contour(Rect r)
        {
            var cont = new List<Point2f>();
            int ox = r.Location.X;
            int oy = r.Location.Y;
            cont.Add(new Point2f(ox, oy));
            cont.Add(new Point2f(ox, oy + r.Height));
            cont.Add(new Point2f(ox + r.Width, oy + r.Height));
            cont.Add(new Point2f(ox + r.Width, oy));
            return cont;
        }
        
        private void unwarpSudokuSquare()
        {
            var contour = orderFourPoints(likelyCandidate);

            var src = new Point2f[] { contour[0], contour[1], contour[2], contour[3] };
            var dest = new Point2f[] { new Point2f(0, 0), new Point2f(0, 432), new Point2f(432, 432), new Point2f(432, 0) };

            var transform = Cv2.GetPerspectiveTransform(src, dest);
            var fs1 = transform.Inv();

            Cv2.WarpPerspective(gray, unwarpedSudoku, transform, new Size(432, 432));

            var fun = Cv2.PerspectiveTransform(unwarpedcentres, fs1);

            int j = 0;
            foreach (var f in fun)
            {
                centres[j] = new SKPoint(f.X, f.Y);
                j++;
            }
            
        }

        private Point[] orderFourPoints(Point[] points)
        {
            Point[] sorted = new Point[4];

            int largestSum = 0, largestDiff = 0;
            int smallestSum = 100000, smallestDiff = 100000;

            foreach (Point p in points)
            {
                int sum = p.X + p.Y;
                int diff = p.X - p.Y;
                if (sum > largestSum)
                {
                    sorted[2] = p;
                    largestSum = sum;
                }
                if (sum < smallestSum)
                {
                    sorted[0] = p;
                    smallestSum = sum;
                }
                if (diff > largestDiff)
                {
                    sorted[3] = p;
                    largestDiff = diff;
                }
                if (diff < smallestDiff)
                {
                    sorted[1] = p;
                    smallestDiff = diff;
                }
            }
            return sorted;
        }

        private bool findContourOfPuzzle()
        {
            Point[] simple;
            double maxArea = 0;
            double area;
            bool found = false;

            foreach (Point[] contour in contours)
            {                
                var curve = new List<Point>(contour);
                double eps = Cv2.ArcLength(curve, true) * .08;
                simple = Cv2.ApproxPolyDP(curve, eps, true);
                if (simple.Length == 4 && Cv2.IsContourConvex(simple))
                {
                    area = Cv2.ContourArea(contour);
                    if (area > maxArea)
                    {
                        likelyCandidate = simple;
                        maxArea = area;
                        found = true;
                    }
                }
            }
            return found;
        }

        public byte[] trainSVM(double[][] inputs, int[] outputs)
        {
            IKernel kernel = Gaussian.Estimate(inputs, inputs.Length / 4);

            var numComplexity = kernel.EstimateComplexity(inputs);

            double complexity = numComplexity*8;
            double tolerance = (double)0.2;
            int cacheSize = (int)1000;
            SelectionStrategy strategy = SelectionStrategy.SecondOrder;

            // Create the learning algorithm using the machine and the training data
            var ml = new MulticlassSupportVectorLearning<IKernel>()
            {
                // Configure the learning algorithm
                Learner = (param) => new SequentialMinimalOptimization<IKernel>()
                {
                    Complexity = complexity,
                    Tolerance = tolerance,
                    CacheSize = cacheSize,
                    Strategy = strategy,
                    Kernel = kernel
                }
            };

            var ksvm = ml.Learn(inputs, outputs);
            byte[] saved;
            Accord.IO.Serializer.Save(ksvm, out saved);
            return saved;
        }

        public void loadSVM(byte[] kernelbyteArray)
        {
            Accord.IO.Serializer.Load(kernelbyteArray, out saveKern);
        }
        public void loadSVM(string kernelPath)
        {
            Accord.IO.Serializer.Load(kernelPath, out saveKern);
        }

        public Sudoku OCR(List<Mat> numbers)
        {
            double[][] testdata = new double[81][];
            int[] outputs = new int[81];
            double[] probab = new double[81];
            int i = 0;
            foreach (var d in numbers)
            {
                if (d == null)
                {
                    testdata[i] = null;
                } else
                {
                    int size = d.Size().Height * d.Size().Height;
                    byte[] managedArray = new byte[size];
                    Marshal.Copy(d.Data, managedArray, 0, size);

                    testdata[i] = (Array.ConvertAll(managedArray, c => c != 0 ? (double)1 : 0));
                }
                i++;
            }
            i = 0;
            foreach (var t in testdata)
            {
                if (t != null)
                {
                    outputs[i] = saveKern.Decide(t);
                    probab[i] = saveKern.Probability(t);
                }
                else
                {
                    outputs[i] = 0;
                    probab[i] = -1;
                }
                i++;
            }

            for (int j = 0; j < 81; j++)
            {
                if (probab[j] < 0.2)
                {
                    outputs[j] = 0;
                }
            }
            return new Sudoku(outputs, probab);
        }
    }

}