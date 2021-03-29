using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Laboratory_1
{
    public partial class Form1 : Form
    {
        Bitmap image;
        bool can_correcting;
        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files|*.png;*jpg;|All files (*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
        }

        private void inverseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
                image = newImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void gaussianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_ProgressChanged_1(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted_1(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void grayWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayWorldFilter(image);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void blurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayscaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void verticalWavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new VerticalWavesFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void sharpnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void glassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GlassFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void horizontalWavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new HorizontalWavesFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters sepia = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(sepia);
        }

        private void openingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Opening();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void closingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Closing();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void gradToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Dilation();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void erosionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Erosion();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void gradToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filters filter = new Grad();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void correctionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("For correction click on picture", "Message",MessageBoxButtons.OK);
            can_correcting = true;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (can_correcting)
            {
                Filters filter = new Correcting(((Bitmap)pictureBox1.Image).GetPixel(e.Location.X, e.Location.Y));
                backgroundWorker1.RunWorkerAsync(filter);
                can_correcting = false;
            }
        }

        private void medianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void streeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new StretchingFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }
    }
}
