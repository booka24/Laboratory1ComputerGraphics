using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace Laboratory_1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        virtual public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                                                255 - sourceColor.G,
                                                255 - sourceColor.B);
            return resultColor;

        }
    }
    class VerticalWavesFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(Clamp((int)(x + 20 * Math.Sin(2 * Math.PI * x / 30)), 0, sourceImage.Width - 1), y);
        }
    }
    class GrayWorldFilter : Filters
    {
        int avR;
        int avG;
        int avB;
        int average;

        public GrayWorldFilter(Bitmap srcImg)
        {
            avR = avG = avB = 0;

            for (int i = 0; i < srcImg.Width; i++)
                for (int j = 0; j < srcImg.Height; j++)
                {
                    avR += srcImg.GetPixel(i, j).R;
                    avG += srcImg.GetPixel(i, j).G;
                    avB += srcImg.GetPixel(i, j).B;
                }

            int pixelsQty = srcImg.Width * srcImg.Height;
            avR /= pixelsQty;
            avG /= pixelsQty;
            avB /= pixelsQty;
            average = (avR + avG + avB) / 3;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            return Color.FromArgb(Clamp((sourceColor.R * average / avR), 0, 255),
            Clamp((sourceColor.G * average / avG), 0, 255),
            Clamp((sourceColor.B * average / avB), 0, 255));
        }
    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)

                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];


                }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));


        }
    }
    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float normal = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                    normal += kernel[i + radius, j + radius];

                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= normal;

        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }

    class GrayscaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color pixelColor = sourceImage.GetPixel(x, y);
            int grayScale = (int)((pixelColor.R * .299) + (pixelColor.G * .587) + (pixelColor.B * .114));
            return Color.FromArgb(pixelColor.A, grayScale, grayScale, grayScale);
        }
    }
    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            kernel = new float[3,3]{ { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };

        }
    }
    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int k = 9;
            int intensity = Convert.ToInt32((sourceColor.R * 0.299) + (sourceColor.G * 0.587) + (sourceColor.B * 0.114));
            int R = intensity + 2 * k;
            int G = Convert.ToInt32(intensity + 0.5 * k);
            int B = intensity - 1 * k;

            Color resultColor = Color.FromArgb(Clamp(R, 0, 255), Clamp(G, 0, 255), Clamp(B, 0, 255));
            return resultColor;
        }
    }
    class GlassFilter : Filters
    {
        Random random;

        public GlassFilter()
        {
            random = new Random();
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(Clamp(x + (int)((random.NextDouble() - 0.5) * 10), 0, sourceImage.Width - 1),
                                        Clamp(y + (int)((random.NextDouble() - 0.5) * 10), 0, sourceImage.Height - 1));
        }
    }
    class HorizontalWavesFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(Clamp((int)(x + 20 * Math.Sin(2 * Math.PI * y / 60)), 0, sourceImage.Width - 1), y);
        }

    }

    class Morphology : Filters
    {
        protected int[,] Matrix;
        protected Morphology() { }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int i, int j)
        {
            throw new NotImplementedException();
        }
    }
    class Dilation : Morphology
    {
        public Dilation()
        {
            Matrix = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int X = Matrix.GetLength(0) / 2;
            int Y = Matrix.GetLength(1) / 2;

            float R = 0, G = 0,B = 0;
            for (int l = -Y; l <= Y; ++l)
                for (int k = -X; k <= X; ++k)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    if ((Matrix[k + X, l + Y] == 1) && (neighborColor.R > R))
                        R = neighborColor.R;
                    if ((Matrix[k + X, l + Y] == 1) && (neighborColor.G > G))
                        G = neighborColor.G;
                    if ((Matrix[k + X, l + Y] == 1) && (neighborColor.B > B))
                        B = neighborColor.B;
                }
            return Color.FromArgb(Clamp((int)R, 0, 255),Clamp((int)G, 0, 255),Clamp((int)B, 0, 255));
        }
    }
    class Erosion : Morphology
    {
        public Erosion()
        {
            Matrix = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int X = Matrix.GetLength(0) / 2;
            int Y = Matrix.GetLength(1) / 2;

            float R, G, B;
            R = G = B = 255;

            for (int l = -Y; l <= Y; ++l)
                for (int k = -X; k <= X; ++k)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    if ((Matrix[k + X, l + Y] == 1) && (neighborColor.R < R))
                        R = neighborColor.R;
                    if ((Matrix[k + X, l + Y] == 1) && (neighborColor.G < G))
                        G = neighborColor.G;
                    if ((Matrix[k + X, l + Y] == 1) && (neighborColor.B < B))
                        B = neighborColor.B;
                }
            return Color.FromArgb(Clamp((int)R, 0, 255), Clamp((int)G, 0, 255), Clamp((int)B, 0, 255));

        }
    }
    class Opening : Morphology
    {
        public Opening()
        {
            Matrix = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = sourceImage;
            Filters filter = new Erosion();
            resultImage = filter.processImage(resultImage, worker);
            filter = new Dilation();
            resultImage = filter.processImage(resultImage, worker);
            return resultImage;
        }
    }
    class Closing : Morphology
    {
        public Closing()
        {
            Matrix = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = sourceImage;
            Filters filter = new Dilation();
            resultImage = filter.processImage(resultImage, worker);
            filter = new Erosion();
            resultImage = filter.processImage(resultImage, worker);
            return resultImage;
        }
    }
    class Grad : Morphology
    {
        public Grad()
        {
            Matrix = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = sourceImage;
            Bitmap tmp1 = sourceImage;
            Bitmap tmp2 = sourceImage;
            Filters filter = new Dilation();
            tmp1 = filter.processImage(tmp1, worker);
            filter = new Erosion();
            tmp2 = filter.processImage(tmp2, worker);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    int r = Clamp(tmp1.GetPixel(i, j).R - tmp2.GetPixel(i, j).R, 0, 255);
                    int g = Clamp(tmp1.GetPixel(i, j).G - tmp2.GetPixel(i, j).G, 0, 255);
                    int b = Clamp(tmp1.GetPixel(i, j).B - tmp2.GetPixel(i, j).B, 0, 255);

                    resultImage.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }

            return resultImage;
        }
    }
    class Correcting : Filters
    {
        private Color color;
        public Correcting(Color _color)
        {
            color = _color;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y); //src
            int R, G, B;

            if (color.R == 0)
                R = sourceColor.R;
            else
                R = sourceColor.R * (255 / color.R);

            if (color.G == 0)
                G = sourceColor.G;
            else
                G = sourceColor.G * (255 / color.G);

            if (color.B == 0)
                B = sourceColor.B;
            else
                B = sourceColor.B * (255 / color.B);

            Color resultColor = Color.FromArgb(Clamp(R, 0, 255), Clamp(G, 0, 255), Clamp(B, 0, 255));
            return resultColor;
        }
    }
    class MedianFilter : Filters
    {

        public MedianFilter(){}
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {


            List<int> R = new List<int>();
            List<int> G = new List<int>();
            List<int> B = new List<int>();

            for (int l = -3; l <= 3; ++l)
                for (int k = -3; k <= 3; ++k)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    R.Add(sourceImage.GetPixel(idX, idY).R);
                    G.Add(sourceImage.GetPixel(idX, idY).G);
                    B.Add(sourceImage.GetPixel(idX, idY).B);
                }
            R.Sort();
            G.Sort();
            B.Sort();

            return Color.FromArgb(R[R.Count / 2], G[G.Count / 2], B[B.Count / 2]);
        }
    }
    class StretchingFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color source = sourceImage.GetPixel(x, y);
            byte intensity = (byte)(((0.2125 * source.R + 0.7154 * source.G + 0.0721 * source.B) / 3));
            return Color.FromArgb(Clamp(source.R + intensity, 0, 255), Clamp(source.G + intensity, 0, 255), Clamp(source.B + intensity, 0, 255));
        }
    }
}




