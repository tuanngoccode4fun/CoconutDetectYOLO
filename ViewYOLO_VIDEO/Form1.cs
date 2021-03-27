using Alturos.Yolo;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp.Extensions;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ViewYOLO_VIDEO
{
    public partial class Form1 : Form
    {
        VideoCapture videocapture;
        int yoloWidth = 608, yoloHeight = 608;
        Mat image;
        public Form1()
        {
            InitializeComponent();

        }

        private void BT_START_Click(object sender, EventArgs e)
        {
            if(!backgroundWorker1.IsBusy)
            backgroundWorker1.RunWorkerAsync();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var configurationDetector = new ConfigurationDetector();
            var config = configurationDetector.Detect();
            YoloWrapper yoloWrapper = new YoloWrapper(config);
            // OpenCV & WPF setting

            image = new Mat();

            //WriteableBitmap wb = new WriteableBitmap(yoloWidth, yoloHeight, 96, 96, PixelFormats.Bgr24, null);
            var bgWorker = (BackgroundWorker)sender;
            byte[] imageInBytes = new byte[(int)(yoloWidth * yoloHeight * image.Channels())];
            videocapture = new VideoCapture(textBox1.Text);
            while (!bgWorker.CancellationPending)
            {

                    using (Mat imageOriginal = new Mat())
                    {
                        // read a single frame and convert the frame into a byte array
                        videocapture.Read(imageOriginal);
                        if (imageOriginal.Empty())
                            break;
                        image = imageOriginal.Resize(new OpenCvSharp.Size(yoloWidth, yoloHeight));
                        imageInBytes = image.ToBytes();

                        // conduct object detection and display the result
                        var items = yoloWrapper.Detect(imageInBytes);
                        foreach (var item in items)
                        {
                            var x = item.X;
                            var y = item.Y;
                            var width = item.Width;
                            var height = item.Height;
                            var type = item.Type;  // class name of the object

                            // draw a bounding box for the detected object
                            // you can set different colors for different classes
                            Cv2.Rectangle(image, new OpenCvSharp.Rect(x, y, width, height), Scalar.Green, 3);
                            Cv2.PutText(image, type, new OpenCvSharp.Point(x - 10, y), HersheyFonts.HersheyComplex, 0.5, Scalar.Red);
                        }
                        bgWorker.ReportProgress(0,image);
                        //// display the detection result
                        //WriteableBitmapConverter.ToWriteableBitmap(image, wb);
                        ///* WPF component: videoViewer
                        //<Canvas Name="canvasYoloVideo" Height="608" Width="608">
                        //  <Image Name="videoViewer" Height="608" Width="608" Stretch="Fill" />
                        //</Canvas>
                        //*/
                        //videoViewer.Source = wb;

                    }
            }
            // Read a video file and run object detection over it!
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var frameBitmap = (Mat)e.UserState;
            pictureBoxView.Image?.Dispose();
            pictureBoxView.Image = frameBitmap.ToBitmap();
        }

        private void BT_STOP_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
                backgroundWorker1.CancelAsync();
        }
    }
}
