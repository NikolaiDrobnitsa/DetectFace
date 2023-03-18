using Amazon.S3.Transfer;
using Amazon.S3;
using System;
using System.Collections.Generic;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Amazon.S3.Model;
using Amazon;
using Amazon.Rekognition.Model;
using Amazon.Rekognition;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;

namespace FaceDetect
{
    public partial class Form3 : Form
    {
        //private Button button1;
        //private Button button2;
        //private PictureBox pictureBox1;

        public Form3()
        {
            InitializeComponent();
            LoadS3FilesToListBox();
            //InitializeControls();
        }
        private void InitializeControls()
        {

            //button1 = new Button
            //{
            //    Text = "Upload",
            //    Dock = DockStyle.Top
            //};
            //button1.Click += button1_Click;
            //Controls.Add(button1);
            //button2 = new Button
            //{
            //    Text = "Upload to S3",
            //    Dock = DockStyle.Top
            //};
            //button2.Click += button2_Click;
            //Controls.Add(button2);

            // Create a picture box to display the image with detected faces
            //pictureBox1 = new PictureBox
            //{
            //    SizeMode = PictureBoxSizeMode.Zoom,
            //    Dock = DockStyle.Fill
            //};
            //Controls.Add(pictureBox1);
        }
        private static readonly string bucketName = "nikolyabucket";
        private static string keyName = "imgface.png";
        private static readonly string accessKey = "AKIAV5NLL37Y36O5Q43I";
        private static readonly string secretKey = "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";
        private static readonly RegionEndpoint regionEndpoint = RegionEndpoint.USEast2;
        private async void button2_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = fileDialog.FileName;
                await UploadToS3Async(filePath);
            }
        }
        private async void LoadS3FilesToListBox()
        {
            listBox1.Items.Clear();
            var s3Client = new AmazonS3Client(accessKey, secretKey, regionEndpoint);

            var listObjectsRequest = new ListObjectsRequest
            {
                BucketName = bucketName
            };

            var listObjectsResponse = await s3Client.ListObjectsAsync(listObjectsRequest);

            foreach (var s3Object in listObjectsResponse.S3Objects)
            {
                listBox1.Items.Add(s3Object.Key);
            }
        }
        public static async Task<Stream> DownloadFromS3Async()
        {
            try
            {
                var client = new AmazonS3Client(accessKey, secretKey, regionEndpoint);
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                var response = await client.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error encountered ***. Message:'{ex.Message}' when reading an object");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error encountered on server. Message:'{ex.Message}' when reading an object");
            }
            return null;
        }
        //private async void button1_Click(object sender, EventArgs e)
        //{
        //    var imageStream = await DownloadFromS3Async();
        //    if (imageStream != null)
        //    {
        //        pictureBox1.Image =  Image.FromStream(imageStream);
        //    }
        //}
        private async Task UploadToS3Async(string filePath)
        {
            try
            {
                var client = new AmazonS3Client(accessKey, secretKey, regionEndpoint);
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = Path.GetFileName(filePath),
                    FilePath = filePath
                };
                var response = await client.PutObjectAsync(putRequest);
                //MessageBox.Show($"Upload completed. Request Id: {response.ResponseMetadata.RequestId}");
                LoadS3FilesToListBox();
            }
            catch (AmazonS3Exception ex)
            {
                MessageBox.Show($"Error encountered ***. Message:'{ex.Message}' when uploading an object");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unknown error encountered on server. Message:'{ex.Message}' when uploading an object");
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            show_imgae();
        }
        public async void show_imgae()
        {
            var imageStream = await DownloadFromS3Async();
            if (imageStream != null)
            {
                AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient(accessKey, secretKey, regionEndpoint);

                MemoryStream memoryStream = new MemoryStream();
                imageStream.CopyTo(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                Image image = Image.FromStream(memoryStream);

                DetectFacesRequest detectFacesRequest = new DetectFacesRequest()
                {
                    Image = new Amazon.Rekognition.Model.Image()
                    {
                        Bytes = new MemoryStream(imageBytes)
                    },
                    Attributes = new List<String>()
                    {
                "ALL"
                    }
                };

                DetectFacesResponse detectFacesResponse = await rekognitionClient.DetectFacesAsync(detectFacesRequest);

                if (detectFacesResponse.FaceDetails.Count > 0)
                {
                    // Create a Bitmap object from the original image
                    Bitmap bmp = new Bitmap(image);

                    // Draw white rectangles around detected faces
                    using (Graphics graphics = Graphics.FromImage(bmp))
                    {
                        Pen pen = new Pen(Color.White, 3);
                        foreach (FaceDetail faceDetail in detectFacesResponse.FaceDetails)
                        {
                            graphics.DrawRectangle(pen, faceDetail.BoundingBox.Left * bmp.Width, faceDetail.BoundingBox.Top * bmp.Height, faceDetail.BoundingBox.Width * bmp.Width, faceDetail.BoundingBox.Height * bmp.Height);
                        }
                    }

                    // Display the modified image in the picture box
                    pictureBox1.Image = bmp;
                    //MessageBox.Show("if");

                }
                else
                {
                    // If no faces are detected, display the original image
                    pictureBox1.Image = image;
                    //MessageBox.Show("else");

                }
            }
        } 
        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                keyName = listBox1.SelectedItem.ToString();
                //MessageBox.Show("item =" + listBox1.SelectedItem + "\nkeyname: " + keyName + "\nindex:" + listBox1.SelectedIndex);
                show_imgae();
            }
            else
            {
                MessageBox.Show("Выберите какою-то картинку!");
                show_imgae();
            }

        }
    }
}
