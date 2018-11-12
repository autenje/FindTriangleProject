using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FindTriangles.Models;
using System.Reflection;
using AForge;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

namespace FindTriangles.Controllers
{
    public class TriangleController : ApiController
    {


        Triangle[] triangles = new Triangle[]
        {
            new Triangle {Name = " ", PtAx = 0, PtAy = 0, PtBx = 0, PtBy = 0,PtCx = 0, PtCy = 0, Area = 0, Col = 0, Row = " " }


        };


        // GET: api/Triangle
        public IEnumerable<Triangle> Get()
        {
            return triangles;
        }

        // GET: api/Triangle/5
        public IHttpActionResult Get(int id)
        {
            
            List<Triangle> newTriangleList = ProcessImage();

            return Ok(newTriangleList);

        }

        // POST: api/Triangle
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Triangle/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Triangle/5
        public void Delete(int id)
        {
        }

        private string CheckForUploadedFile()
        {

            String sourceDir =  System.Web.Hosting.HostingEnvironment.MapPath(@"~/UploadedFiles");
            String foundFile = String.Empty;

            string[] fileEntries = System.IO.Directory.GetFiles(sourceDir);
            foreach (string fileName in fileEntries)
            {
                // do something with fileName
               foundFile =  fileName;
            }

                            
            return foundFile;
        }

 

        private List<Triangle> NoTriangleFound()
        {

            List<Triangle> TriangleList = new List<Triangle>();
               {
                   new Triangle { Name = "No Triangle(s) Detected", PtAx = 0, PtAy = 0, PtBx = 0, PtBy = 0, PtCx = 0, PtCy = 0, Area = 0, Col = 0, Row = " " };
               };


        

            return TriangleList;
        }

        //private List<Triangle> LoadImage()
        //{
        //    // load arrow bitmap
        //    string embeddedFileName = CheckForUploadedFile();
        //    Assembly assembly = this.GetType().Assembly;
        //    Bitmap image = new Bitmap(assembly.GetManifestResourceStream(embeddedFileName));
        //    List<Triangle>  triangleList;
        //   // triangleList=  ProcessImage(image);
        //    return triangleList;
        //}

        private List<Triangle> ProcessImage()
        {
            List<Triangle> TriangleList = new List<Triangle>();

            List<ULBounds> triBounds = new List<ULBounds>();

            int currentMaxX = 0;
            int currentMaxY = 0;

            int currentUBoundsX = 0;
            int currentUBoundsY = 0;

            int currentCol = 0;
            String currentRow = String.Empty;

            string embeddedFileName = CheckForUploadedFile();

            if (embeddedFileName == String.Empty)
            {

                TriangleList = NoTriangleFound();
                return TriangleList;

            }

            Bitmap bitmap = (Bitmap)Bitmap.FromFile(embeddedFileName);
            // Triangle Class List


           
            // values needed to find area of a triangle
            // double[] X;
            //  double[] Y;
            int z;
            double area;

            // lock image

            String filePath = CheckForUploadedFile();

            Int16 Tol = 38;
            Color replace1 = new Color();
            replace1 = Color.FromArgb(129, 187, 237);

            Color newColor = new Color();
            newColor = Color.FromArgb(0, 0, 0);

            ImageConverter ic = new ImageConverter();


            Color replace2 = new Color();
            replace2 = Color.FromArgb(199, 221, 234);
            Tol = 38;

                System.Drawing.Image tmpImage = System.Drawing.Image.FromFile(filePath);
                tmpImage = ColorReplace(tmpImage, Tol, replace1, newColor);
                
                ColorReplace(tmpImage, Tol, replace2, newColor);

               // UpdatePictureBoxPositionOutPut();


            bitmap = new Bitmap(tmpImage);

            BitmapData bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite, bitmap.PixelFormat);



            // step 1 - turn background to black
            ColorFiltering colorFilter = new ColorFiltering();

            colorFilter.Red = new IntRange(0, 64);
            colorFilter.Green = new IntRange(0, 64);
            colorFilter.Blue = new IntRange(0, 64);
            colorFilter.FillOutsideRange = false;

            colorFilter.ApplyInPlace(bitmapData);

            // step 2 - locating objects
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 5;
            blobCounter.MinWidth = 5;

            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            bitmap.UnlockBits(bitmapData);

            // step 3 - check objects' type and highlight
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Graphics g = Graphics.FromImage(bitmap);
            Pen yellowPen = new Pen(Color.Yellow, 2); // circles
            Pen redPen = new Pen(Color.Red, 2);       // quadrilateral
            Pen brownPen = new Pen(Color.Brown, 2);   // quadrilateral with known sub-type
            Pen greenPen = new Pen(Color.Green, 2);   // known triangle
            Pen bluePen = new Pen(Color.Blue, 2);     // triangle

            int triFound = 0;

            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);

                DoublePoint center;
                double radius;
                int tempX = 0;
                int tempY = 0;

                int tempCol = 0;
                String tempRow = string.Empty;


                // is circle ?
                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    g.DrawEllipse(yellowPen,
                        (float)(center.X - radius), (float)(center.Y - radius),
                        (float)(radius * 2), (float)(radius * 2));
                }
                else
                {
                    List<IntPoint> corners;

                    // is triangle or quadrilateral
                    if (shapeChecker.IsConvexPolygon(edgePoints, out corners))
                    {
                        // get sub-type
                        PolygonSubType subType = shapeChecker.CheckPolygonSubType(corners);

                        Pen pen;

                        if (subType == PolygonSubType.Unknown)
                        {
                            pen = (corners.Count == 4) ? redPen : bluePen;
                        }
                        else
                        {
                            pen = (corners.Count == 4) ? brownPen : yellowPen;


                            double[] X = { corners[0].X, corners[1].X, corners[2].X };
                            double[] Y = { corners[0].Y, corners[1].Y, corners[2].Y };
                            z = X.Length;

                            area = polygonArea(X, Y, z);

                            if (area < 40000)
                            {
                                triFound = triFound + 1;


                                Triangle tmpTri = new Triangle();
                                ULBounds tmpULB = new ULBounds();
                                tmpTri.Name = "Triangle " + triFound.ToString();
                                tmpTri.PtAx = corners[0].X;
                                tmpTri.PtAy = corners[0].Y;

                                if (tmpTri.PtAx > currentMaxX)
                                {
                                    currentMaxX = tmpTri.PtAx;
                                }

                                if (tmpTri.PtAy > currentMaxY)
                                {
                                    currentMaxY = tmpTri.PtAy;
                                }


                                tmpTri.PtBx = corners[1].X;
                                tmpTri.PtBy = corners[1].Y;

                                if (tmpTri.PtBx > currentMaxX)
                                {
                                    currentMaxX = tmpTri.PtBx;
                                }

                                if (tmpTri.PtBy > currentMaxY)
                                {
                                    currentMaxY = tmpTri.PtBy;
                                }




                                tmpTri.PtCx = corners[2].X;
                                tmpTri.PtCy = corners[2].Y;

                                if (tmpTri.PtCx > currentMaxX)
                                {
                                    currentMaxX = tmpTri.PtCx;
                                }

                                if (tmpTri.PtBy > currentMaxY)
                                {
                                    currentMaxY = tmpTri.PtBy;
                                }

                                if (currentCol == 0)
                                { currentCol++; tempCol = currentCol; currentUBoundsY = currentMaxY; }
                                else
                                { currentCol = GetMaxColumn(triBounds); }

                                if (currentRow == string.Empty)
                                { currentRow = Increment(currentRow); tempRow = currentRow; currentUBoundsX = currentMaxX; }
                                else
                                { currentRow = GetMaxRow(triBounds); };


                                if (triBounds.Count == 0)
                                {
                                    //adding Row
                                    tmpULB.Col = 0;
                                    tmpULB.Row = tempRow;
                                    tmpULB.Lower = 0;
                                    tmpULB.Upper = currentMaxY;

                                    triBounds.Add(tmpULB);

                                    tmpULB = new ULBounds();

                                    //adding Col
                                    tmpULB.Col = tempCol;
                                    tmpULB.Row = String.Empty;
                                    tmpULB.Lower = 0;
                                    tmpULB.Upper = currentMaxX;

                                    triBounds.Add(tmpULB);
                                }

                                if (currentMaxX > (currentUBoundsX * 1.15))
                                {
                                    currentUBoundsX = GetMaxColumnUBounds(triBounds);
                                    if (currentMaxX > (currentUBoundsX * 1.15))
                                    {
                                        tempCol = GetMaxColumn(triBounds);

                                        tmpULB = new ULBounds();
                                        tempCol = tempCol++;

                                        tmpULB.Col = tempCol;
                                        tmpULB.Row = String.Empty;
                                        tmpULB.Lower = currentUBoundsX + 1;
                                        tmpULB.Upper = currentMaxX;
                                        triBounds.Add(tmpULB);
                                    }
                                }



                                if (currentMaxY > (currentUBoundsY * 1.15))
                                {
                                    currentUBoundsY = GetMaxColumnUBounds(triBounds);
                                    if (currentMaxY > (currentUBoundsY * 1.15))
                                    {
                                        tempRow = GetMaxRow(triBounds);
                                        tmpULB = new ULBounds();
                                        tempRow = Increment(tempRow);
                                        //adding Col
                                        tmpULB.Col = tempCol;
                                        tmpULB.Row = String.Empty;
                                        tmpULB.Lower = 0;
                                        tmpULB.Upper = currentMaxX;

                                        triBounds.Add(tmpULB);
                                    }
                                }


                                tmpTri.Row = currentRow.ToUpper();
                                tmpTri.Col = currentCol;

                            
                            tmpTri.Area = polygonArea(X, Y, z);

                           
                            TriangleList.Add(tmpTri);
                            }
                      


                        }

                        g.DrawPolygon(pen, ToPointsArray(corners));
                    }
                }
            }

            yellowPen.Dispose();
            redPen.Dispose();
            greenPen.Dispose();
            bluePen.Dispose();
            brownPen.Dispose();
            g.Dispose();


          
            return TriangleList;
           
         
        }

        private Point[] ToPointsArray(List<IntPoint> points)
        {
            Point[] array = new Point[points.Count];

            for (int i = 0, n = points.Count; i < n; i++)
            {
                array[i] = new Point(points[i].X, points[i].Y);
            }

            return array;
        }


        static double polygonArea(double[] X,
               double[] Y, int n)
        {
            // Initialize area 
            double area = 0.0;

            
            // formula area
            int j = n - 1;
            for (int i = 0; i < n; i++)
            {
                area += (X[j] + X[i]) *
                            (Y[j] - Y[i]);

                
                j = i;
            }

            // Return absolute value 
            return Math.Abs(area / 2.0);
        }

        public int GetMax(int x, int y, int z)
        {

            return Math.Max(x, Math.Max(y, z));
        }

        private System.Drawing.Image ColorReplace(System.Drawing.Image inputImage, int tolerance, Color oldColor, Color NewColor)
        {
            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height);
            Graphics G = Graphics.FromImage(outputImage);
            G.DrawImage(inputImage, 0, 0);
            for (Int32 y = 0; y < outputImage.Height; y++)
                for (Int32 x = 0; x < outputImage.Width; x++)
                {
                    Color PixelColor = outputImage.GetPixel(x, y);
                    if (PixelColor.R > oldColor.R - tolerance && PixelColor.R < oldColor.R + tolerance && PixelColor.G > oldColor.G - tolerance && PixelColor.G < oldColor.G + tolerance && PixelColor.B > oldColor.B - tolerance && PixelColor.B < oldColor.B + tolerance)
                    {
                        int RColorDiff = oldColor.R - PixelColor.R;
                        int GColorDiff = oldColor.G - PixelColor.G;
                        int BColorDiff = oldColor.B - PixelColor.B;

                        if (PixelColor.R > oldColor.R) RColorDiff = NewColor.R + RColorDiff;
                        else RColorDiff = NewColor.R - RColorDiff;
                        if (RColorDiff > 255) RColorDiff = 255;
                        if (RColorDiff < 0) RColorDiff = 0;
                        if (PixelColor.G > oldColor.G) GColorDiff = NewColor.G + GColorDiff;
                        else GColorDiff = NewColor.G - GColorDiff;
                        if (GColorDiff > 255) GColorDiff = 255;
                        if (GColorDiff < 0) GColorDiff = 0;
                        if (PixelColor.B > oldColor.B) BColorDiff = NewColor.B + BColorDiff;
                        else BColorDiff = NewColor.B - BColorDiff;
                        if (BColorDiff > 255) BColorDiff = 255;
                        if (BColorDiff < 0) BColorDiff = 0;

                        outputImage.SetPixel(x, y, Color.FromArgb(RColorDiff, GColorDiff, BColorDiff));
                    }
                }
            return outputImage;
        }


        static string Increment(string s)
        {

            // first case - string is empty: return "a"

            if ((s == null) || (s.Length == 0))

                return "a";

            // next case - last char is less than 'z': simply increment last char

            char lastChar = s[s.Length - 1];

            string fragment = s.Substring(0, s.Length - 1);

            if (lastChar < 'z')
            {

                ++lastChar;

                return fragment + lastChar;

            }

            // next case - last char is 'z': roll over and increment preceding string

            return Increment(fragment) + 'a';

        }


        private int GetMaxColumn(List<ULBounds> triList)
        {
            int maxCol = 0;
            var tempTri = triList.Where(t => t.Row == "").OrderByDescending(t => t.Upper).First();


            maxCol = tempTri.Col;

            return maxCol;
        }

        private String GetMaxRow(List<ULBounds> triList)
        {
            String maxRow = string.Empty;
            var tempTri = triList.Where(t => t.Col < 1).OrderByDescending(t => t.Upper).First();


            maxRow = tempTri.Row;

            return maxRow;
        }

        private int GetMaxColumnUBounds(List<ULBounds> triList)
        {
            int maxCol = 0;
            var tempTri = triList.Where(t => t.Row == "").OrderByDescending(t => t.Upper).First();


            maxCol = tempTri.Upper;

            return maxCol;
        }

        private int GetMaxRowUBounds(List<ULBounds> triList)
        {
            int maxCol = 0;
            var tempTri = triList.Where(t => t.Col < 1).OrderByDescending(t => t.Upper).First();


            maxCol = tempTri.Upper;

            return maxCol;
        }

    }
}
