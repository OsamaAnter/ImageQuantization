using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;
using Priority_Queue;
///Algorithms Project
///Intelligent Scissors
///



namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue 
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    public struct Edge
    {
        //return MST
        public float weight;
        public int v1, v2;
    }
    public struct returnFromFunction
    {
        // return List distinct Color
        public List<int> distinct_values;
        public Dictionary<int, RGBPixel> pixelss;
    }
    public struct retrunFromcalc_weight
    {
        public Dictionary<int, List<float>> graph;
        //public PriorityQueue<KeyValuePair<int, int>> priorityQ;
        public List<KeyValuePair<KeyValuePair<int, int>, float>> priorityList;

        public KeyValuePair<KeyValuePair<int, int>, float> minWeight;

        //public Dictionary<int, List<Dictionary<int, float>>> node_edge;

        //public List<node> node_edge;
    }
    public struct test
    {
        public int p; //parent 

        public List<KeyValuePair<int, float>> info;  // <Child , cost>
    }

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>


        //to calculat Distinct Pixels in Image O(N^2)
        public static List<int> FindDistinctBetweenPixels(RGBPixel[,] ImageMatrix)
        {
            Dictionary<int, RGBPixel> Pixels = new Dictionary<int, RGBPixel>();
            int widthImage = GetWidth(ImageMatrix);
            HashSet<int> colorsDistinct = new HashSet<int>();
            int heightImage = GetHeight(ImageMatrix);
            int pixelheight = 0; // loopCounter
            while ( pixelheight < heightImage) // O(n^2)
            {   
                int pixelWidth = 0;
                while ( pixelWidth < widthImage) // O(n)
                {
                    int red, green, blue , PxelSum ;
                    red = ImageMatrix[pixelheight, pixelWidth].red;//O(1)   255
                    green = ImageMatrix[pixelheight, pixelWidth].green;//O(1) 250
                    blue = ImageMatrix[pixelheight, pixelWidth].blue;//O(1)  100 
                    PxelSum = (red << 16) + (green << 8) + blue;
                    colorsDistinct.Add(PxelSum); // O(1)  
                    pixelWidth+=1; // O(1)
                }
                pixelheight+=1;
            }
            return colorsDistinct.ToList();
        }


        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;
            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        // Calculate MST Cost  
        public static float calWegitDisplay(List<Edge> mst)
        {
            float weghit = 0; 
            foreach (var edge in mst)
            {
                weghit = weghit + edge.weight;
            }
            return weghit;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }

        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>


        //public static List<KeyValuePair<int, List<KeyValuePair<int, float>>>> nodes_keys = 
        //new List<KeyValuePair<int, List<KeyValuePair<int, float>>>>();

        //public static List<KeyValuePair<int, float>> nodes_value = new List<KeyValuePair<int, float>>();
        //public static List<Dictionary<int, float>> node_dict = new List<Dictionary<int, float>>();
        public static List<test> ConstructGraph(List<int> distinct_values)
        {
            float result = 0; //O(1)
            int numberOfDistinct = distinct_values.Count;//O(1)

            List<test> test_output = new List<test>(numberOfDistinct);//O(1)

            for (int i = 0; i < numberOfDistinct; i++)//n
            {
                byte r1 = (byte)(distinct_values[i] >> 16);//O(1)
                byte g1 = (byte)(distinct_values[i] >> 8);//O(1)
                byte b1 = (byte)(distinct_values[i]);//O(1)

                test res = new test();//O(1)
                res.p = distinct_values[i];//O(1)
                List<KeyValuePair<int, float>> res_list = new List<KeyValuePair<int, float>>();//O(1)

                //List<KeyValuePair<int, float>> temp_list = new List<KeyValuePair<int, float>>();//O(1)
                //List < Dictionary<int, float> > edge_list = new List<Dictionary<int, float>>();//O(1)
                for (int j = i + 1; j < numberOfDistinct; j++) //n
                {
                    if (i == j)//O(1)
                    {
                        continue;//O(1)
                    }
                    byte r2 = (byte)(distinct_values[j] >> 16);//O(1)
                    byte g2 = (byte)(distinct_values[j] >> 8);//O(1)
                    byte b2 = (byte)(distinct_values[j]);//O(1)
                    result = (float)Math.Sqrt(((r1 - r2) * (r1 - r2)) + ((g1 - g2) * (g1 - g2)) + ((b1 - b2) * (b1 - b2)));//O(1)

                    KeyValuePair<int, float> out_dict = new KeyValuePair<int, float>(distinct_values[j], result);//O(1)
                    //out_dict.Add(distinct_values[j], result);
                    res_list.Add(out_dict);//O(1)

                    //check if that the minimum edge

                }
                res.info = res_list;//O(1)

                test_output.Add(res);//O(1)

            }

            return test_output;
        }

        private static void DepthFirstSearchAlgorithm( Dictionary<int, bool> visited,
            Dictionary<int, List<int>> neighbours, HashSet<int> cluster , int current)
        {
            // Add pixel in cluster 
            cluster.Add(current);
            // pixel visited
            visited[current] = true;
            // loop in neighbours for pixel and repeat DFS Algorithm 
            foreach (var neighbour in neighbours[current])
            {
                if (visited[neighbour] == false)
                {
                     DepthFirstSearchAlgorithm( visited, neighbours, cluster, neighbour);
                }
                else
                {
                    continue;
                }
            }
        }



        public static RGBPixel[,] QuantizeImage(Dictionary<int, int> palette_dict , RGBPixel[,] ImagePixels)
        {
            
            int YaxisCounter = 0;
            while (YaxisCounter < ImagePixels.GetLength(0)) //height
            {
                int XaxisCounter = 0;
                while ( XaxisCounter < ImagePixels.GetLength(1)) //width
                {
                    int red, green, blue;
                    red = ImagePixels[YaxisCounter, XaxisCounter].red;
                    green = ImagePixels[YaxisCounter, XaxisCounter].green;
                    blue = ImagePixels[YaxisCounter, XaxisCounter].blue;
                    
                    ImagePixels[YaxisCounter, XaxisCounter].red = (byte)(palette_dict[(red << 16) + (green << 8) + blue] >> 16);
                    ImagePixels[YaxisCounter, XaxisCounter].green = (byte)(palette_dict[(red << 16) + (green << 8) + blue] >> 8);
                    ImagePixels[YaxisCounter, XaxisCounter].blue = (byte)(palette_dict[(red << 16) + (green << 8) + blue]);
                    XaxisCounter++;
                }
                YaxisCounter++;
            }

            return ImagePixels;
        }

        //extrct minumum spaning tree
        public static List<Edge> extrctMSTFromGraphByPrim(List<test> output, List<int> distinct_values)
        {
            int sizeOfGraph = distinct_values.Count; //O(1)
           
            FastPriorityQueue<ChildParent> toSortedGraph = new FastPriorityQueue<ChildParent>(sizeOfGraph); //O(1)

            List<Edge> MSTFromGraph = new List<Edge>(); //O(1)

            for (int i = 0; i < sizeOfGraph; i++) // O(n)
            {

                if (i == 0) //O(1)
                {
                    toSortedGraph.Enqueue(new ChildParent(-1, distinct_values[0]), 0);//O(log(n))
                }
                else
                {
                    toSortedGraph.Enqueue(new ChildParent(-1, distinct_values[i]), float.MaxValue);//O(log(n))
                }
            }

            ChildParent minCP; //O(1)
            while (true) //while only O(1) becouse it fixed count but why body //O(nlog(n))
            {
                minCP = toSortedGraph.Dequeue();//O(1)
                if (minCP.parent >= 0)
                {
                    Edge edgetmp;//O(1)
                    edgetmp.v1 = minCP.parent;//O(1)
                    edgetmp.v2 = minCP.child;//O(1)
                    edgetmp.weight = minCP.Priority;//O(1)
                    MSTFromGraph.Add(edgetmp);//O(1)
                }

                foreach (var item in toSortedGraph) //O(N)
                {

                    // to calculate distance btween tow pixels
                    byte redV1, redV2, greenV1, greenV2,blueV1, blueV2;//O(1)
                    redV1 = (byte)(minCP.child >> 16);//O(1)
                    redV2 = (byte)(item.child >> 16);//O(1)
                    greenV1 = (byte)(minCP.child >> 8);//O(1)
                    greenV2 = (byte)(item.child >> 8);//O(1)
                    blueV1 = (byte)(minCP.child);//O(1)
                    blueV2 = (byte)(item.child);//O(1)
                    float Distance = (redV2 - redV1) * (redV2 - redV1) + (greenV2 - greenV1) * (greenV2 - greenV1) + (blueV2 - blueV1) * (blueV2 - blueV1);//O(1)
                    float cal_w = (float)Math.Sqrt(Distance);//O(1)
                    // end 
                    if (cal_w < item.Priority)
                    {
                        item.parent = minCP.child;//O(1)
                        toSortedGraph.UpdatePriority(item, cal_w);//O(log(n))
                    }
                }

                if (toSortedGraph.Count == 0)//O(1)
                {
                    break;//O(1)
                }
                else
                {
                    continue;//O(1)
                }
            }

            return MSTFromGraph;//O(1)
        }

        public static Dictionary<int, int> Cluster(int kCluster,List<int> distinct_values, List<Edge> MSTFromGraph)
        {
            List<HashSet<int>> clusters = new List<HashSet<int>>();
           
            Dictionary<int, bool> visited = new Dictionary<int, bool>();

            Dictionary<int, List<int>> neighbours = new Dictionary<int, List<int>>();
            List<int> neighbours_keys = new List<int>();

            //initialize visited dictionary
            for (int i = 0; i < distinct_values.Count; i++)  // O(n)
            {
                visited.Add(distinct_values[i], false);
            }

            int IndecMax = 0;
            float weightMAx = 0;
            for (int j = 0; j < kCluster - 1; j++)
            {
                weightMAx = 0;
                Edge edgeVar;
                IndecMax = 0;
                for (int i = 0; i < MSTFromGraph.Count; i++)
                {
                    if (MSTFromGraph[i].weight > weightMAx)
                    {
                        weightMAx = MSTFromGraph[i].weight;
                        IndecMax = i;
                        continue;
                    }
                }
                //update the node with the haighest weight (update weight = 0)
                edgeVar.weight = 0;
                edgeVar.v1 = MSTFromGraph[IndecMax].v1;
                edgeVar.v2 = MSTFromGraph[IndecMax].v2;
                MSTFromGraph[IndecMax] = edgeVar;
            }

            for (int i = 0; i < MSTFromGraph.Count; i++)
            {
                if (MSTFromGraph[i].weight > 0)
                {
                    if (neighbours.ContainsKey(MSTFromGraph[i].v1))
                    {
                        neighbours[MSTFromGraph[i].v1].Add(MSTFromGraph[i].v2);
                        // v2
                        if (neighbours.ContainsKey(MSTFromGraph[i].v2))
                        {
                            neighbours[MSTFromGraph[i].v2].Add(MSTFromGraph[i].v1);
                        }
                        else
                        {
                            List<int> tmpList = new List<int>();
                            tmpList.Add(MSTFromGraph[i].v1);
                            neighbours_keys.Add(MSTFromGraph[i].v2);
                            neighbours.Add(MSTFromGraph[i].v2, tmpList);
                        }
                    }
                    else
                    {
                        List<int> listTmp = new List<int>();
                        listTmp.Add(MSTFromGraph[i].v2);
                        neighbours.Add(MSTFromGraph[i].v1, listTmp);
                        neighbours_keys.Add(MSTFromGraph[i].v1);
                        // v2
                        if (neighbours.ContainsKey(MSTFromGraph[i].v2))
                        {
                            if (MSTFromGraph[i].weight != 0)
                            {
                                neighbours[MSTFromGraph[i].v2].Add(MSTFromGraph[i].v1);
                            }
                        }
                        else
                        {
                            listTmp = new List<int>();
                            listTmp.Add(MSTFromGraph[i].v1);
                            neighbours.Add(MSTFromGraph[i].v2, listTmp);
                            neighbours_keys.Add(MSTFromGraph[i].v2);
                        }
                    }
                    continue;
                }
                    if (neighbours.ContainsKey(MSTFromGraph[i].v1)==false)
                    {
                        List<int> listMSt = new List<int>();
                        neighbours_keys.Add(MSTFromGraph[i].v1);
                        neighbours.Add(MSTFromGraph[i].v1, listMSt);
                    }
                    if (neighbours.ContainsKey(MSTFromGraph[i].v2)==false)
                    {
                        List<int> listmst = new List<int>();
                        neighbours.Add(MSTFromGraph[i].v2, listmst);
                        neighbours_keys.Add(MSTFromGraph[i].v2);
                    }
            }
            for (int i = 0; i < neighbours_keys.Count; i++)
            {

                if (visited[neighbours_keys[i]]==false)
                {
                    HashSet<int> tmp500 = new HashSet<int>();
                    DepthFirstSearchAlgorithm( visited, neighbours, tmp500 , neighbours_keys[i]);
                    clusters.Add(tmp500);
                    continue;
                    //cluster_list.Add(h.ToList());
                }
            }

            //color palette
            Dictionary<int, int> palette_dict = new Dictionary<int, int>(); //O(1)
            //List<int> temp = new List<int>();
            Dictionary<int, int> PalleteRes = new Dictionary<int, int>();//O(1)

            //acess list of clusters
            foreach (var v in clusters)
            {

                int red = 0, green = 0, blue = 0;//O(1)
                int average = 0;//O(1)

                for (int i = 0; i < v.Count; i++)
                {
                    palette_dict.Add(v.ToList()[i], (byte)(v.Average()));//O(1)
                    //temp.Add((byte)(v.Average()));
                }
            }

            foreach (var set in clusters)
            {
                int red = 0, green = 0,blue = 0;
                int value = 0;
                foreach (var unit in set)
                {
                    green = (green + (byte)(unit >> 8));
                    red = (red + (byte)(unit >> 16));
                    blue = (blue + (byte)(unit));
                }
                red = (red / set.Count);
                green = (green / set.Count);
                blue = (blue / set.Count);
                value = (red << 16) + (green << 8) + (blue);
                foreach (var unit in set)
                {
                    PalleteRes.Add(unit, value);
                }
            }


            List<int> test = new List<int>();
            List<int> test_count = new List<int>();
            //List<int> test_node = new List<int>();
            int count = 0;
            foreach (var set in clusters)
            {
                foreach (var unit in set)
                {
                    test.Add(unit);
                    count++;
                }
                test_count.Add(count);
            }
            return PalleteRes;
        }


        public static Dictionary<int, int> palette(List<int> test, List<int> test_count)
        {
            Dictionary<int, int> palette_dict = new Dictionary<int, int>();//O(1)
            int index = 0, row_cntr = 0;//O(1)
            int r = 0, g = 0, b = 0, val = 0;//O(1)
            List<int> averages = new List<int>();//O(1)
            for (int i = 0; i < test.Count; i++)//O(n)
            {
                g = (g + (byte)(test[i] >> 8));//O(1)
                r = (r + (byte)(test[i] >> 16));//O(1)
                b = (b + (byte)(test[i]));//O(1)


                if (row_cntr == test_count[index] - 1)//O(1)
                {
                    r = (r / test_count[index]);//O(1)
                    g = (g / test_count[index]);//O(1)
                    b = (b / test_count[index]);//O(1)
                    val = (r << 16) + (g << 8) + (b);//O(1)
                    averages.Add(val);//O(1)
                    r = 0; g = 0; b = 0;//O(1)
                    index++;//O(1)
                    row_cntr = 0;//O(1)
                    //continue;
                }
                row_cntr++;//O(1)
            }

            row_cntr = 1;//O(1)
            index = 0;//O(1)
            for (int i = 0; i < test.Count; i++)//O(n)
            {
                palette_dict.Add(test[i], averages[index]);//O(1)
                if (row_cntr == test_count[index])//O(1)
                {
                    row_cntr = 1;//O(1)
                    index++;//O(1)
                    continue;//O(1)
                }
                row_cntr++;//O(1)
            }
            return palette_dict;//O(1)
        }
        

        public static double calculatingDistancesBetweenTwoPixel(RGBPixel p1, RGBPixel p2)
        {
            double distances = 0;
            int red = p1.red - p2.red;
            int green = p1.green - p2.green;
            int blue = p1.blue - p2.blue;

            distances = Math.Sqrt((red * red) + (green * green) + (blue * blue));

            return distances;
        }

        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }


    }
}