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
        virtual public Bitmap processImage(Bitmap sourceImage, backgroundWorker worker)

        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
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


        internal Bitmap processImage(Bitmap sourceImage)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            return resultImage;
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
                float[,] res = new float[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };

            }
            public void createSharpnessKernel(float[,] res)
            {
                kernel = new float[3, 3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        kernel[i, j] = res[i, j];
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
    }
    class VerticalWavesFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(Clamp((int)(x + 20 * Math.Sin(2 * Math.PI * x / 30)), 0, sourceImage.Width - 1), y);
        }
    }
}




