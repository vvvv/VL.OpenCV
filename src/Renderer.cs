using OpenCvSharp;
using OpenCvSharp.UserInterface;
using System;
using System.Drawing;
using System.Reactive.Subjects;
using System.Windows.Forms;
using VL.Core.Properties;

namespace VL.OpenCV
{
    public partial class Renderer : Form, IDisposable
    {
        PictureBoxIpl pictureBox;

        public Subject<Rectangle> BoundsChanged { get; }

        private Mat image;
        private System.Drawing.Size previousSize;
        private bool enabled = true;

        public Mat Image
        {
            get { return image; }
            set
            {
                if (enabled)
                {
                    image = value;
                    pictureBox.ImageIpl = image; //this could be a potential bottle neck
                    if (previousSize.Width != image.Width || previousSize.Height != image.Height)
                    {
                        previousSize = new System.Drawing.Size(image.Width, image.Height);
                        HandleResize();
                    }
                }
            }
        }

        private bool sizeFromImage = true;

        public bool SizeFromImage
        {
            get { return sizeFromImage; }
            set {
                sizeFromImage = value;
                HandleResize();
            }
        }

        public Renderer()
        {
            BoundsChanged = new Subject<Rectangle>();
            InitializeComponent();
            pictureBox = new PictureBoxIpl();
            SetSize(new Rectangle(1200, 50, 512, 512));
            Show();
        }

        private void Renderer_Load(object sender, EventArgs e)
        {
            pictureBox.ImageIpl = this.Image;
            Controls.Add(pictureBox);
        }

        public void Enable(bool enabled)
        {
            this.enabled = enabled;
            Visible = enabled;
        }

        private void HandleResize()
        {
            if (image == DefaultMat.Damon)
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Normal;
                if (ClientSize.Width != 512 || ClientSize.Height != 512)
                    ClientSize = pictureBox.ClientSize = new System.Drawing.Size(512, 512);
                if (sizeFromImage)
                    MaximumSize = MinimumSize = SizeFromClientSize(ClientSize);
            }
            else
            {
                if (sizeFromImage)
                {
                    if (ClientSize.Width != image.Width || ClientSize.Height != image.Height)
                    {
                        ClientSize = pictureBox.ClientSize = new System.Drawing.Size(image.Width, image.Height);
                        MaximumSize = MinimumSize = SizeFromClientSize(ClientSize);
                    }
                }
                else
                {
                    MinimumSize = MaximumSize = new System.Drawing.Size(0, 0);
                    pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox.ClientSize = ClientSize;
                }
            }
        }

        public void SetSize(Rectangle bounds)
        {
            var boundsinPix = Settings.DIPToPixel(bounds);
            if (boundsinPix != Bounds)
                Bounds = boundsinPix;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            BoundsChanged.OnNext(Settings.DIP(Bounds));
            pictureBox.ClientSize = ClientSize;
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            BoundsChanged.OnNext(Settings.DIP(Bounds));
        }

        public void Dispose()
        {
            Close();
            pictureBox.Dispose();
            base.Dispose();
        }
    }
}
