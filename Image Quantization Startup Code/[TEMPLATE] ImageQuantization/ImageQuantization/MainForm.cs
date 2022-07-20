using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }
        
        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            DateTime before = DateTime.Now;
            //given 
            double sigma = double.Parse(txtGaussSigma.Text);

            //store Colours Count and show it in Alert
            List<int> listDistinctPixels = ImageOperations.FindDistinctBetweenPixels(ImageMatrix);
            MessageBox.Show("Pixcels Colour Distinct Count: " + listDistinctPixels.Count.ToString());

            //Store Construct Graph
            List<test> graph = ImageOperations.ConstructGraph(listDistinctPixels);

            //Minimum Spaning tree quantization
            List<Edge> mst = ImageOperations.extrctMSTFromGraphByPrim(graph, listDistinctPixels);

            //calc weigt to walk in smal road and print it
            float total_cost = ImageOperations.calWegitDisplay(mst);
            MessageBox.Show("MST Total Cost = " + total_cost.ToString());
            int maskSize = (int)nudMaskSize.Value;

            Dictionary<int, int> cluster = ImageOperations.Cluster(maskSize,listDistinctPixels, mst);

            ImageMatrix = ImageOperations.QuantizeImage(cluster , ImageMatrix);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            DateTime after = DateTime.Now;

            TimeSpan ExTime = after - before;
            MessageBox.Show("Execution Time " + ExTime.ToString());


        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void nudMaskSize_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}