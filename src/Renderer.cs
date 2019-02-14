using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.UserInterface;
using System;
using System.Drawing;
using System.Reactive.Subjects;
using System.Windows.Forms;
using VL.Core.Properties;
using System.Runtime.InteropServices;

namespace VL.OpenCV
{
    public enum RendererMode
    {
        SizeFromImage,
        FreeTransform,
        AspectRatioScale
    }

    public partial class Renderer : Form, IDisposable
    {
        //double so division keeps decimal points
        double widthRatio = 1;
        double heightRatio = 1;

        const int WM_SIZING = 0x214;
        const int WMSZ_LEFT = 1;
        const int WMSZ_RIGHT = 2;
        const int WMSZ_TOP = 3;
        const int WMSZ_BOTTOM = 6;

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        PictureBoxIpl pictureBox;

        public Subject<Rectangle> BoundsChanged { get; }

        private CvImage image;
        private System.Drawing.Size previousSize;
        private bool enabled = true;
        private int imageID = 0;
        private int prevWidth = 0;
        private int prevHeight = 0;

        public CvImage Image
        {
            get { return image; }
            set
            {
                if (enabled && value != image)
                {
                    image = value;
                    widthRatio = image.Cols;
                    heightRatio = image.Rows;
                    RefreshIplImage(image?.Mat);
                    if (image != null && (previousSize.Width != image.Width || previousSize.Height != image.Height))
                    {
                        previousSize = new System.Drawing.Size(image.Width, image.Height);
                        HandleResize();
                    }
                }
            }
        }

        public void RefreshIplImage(Mat img)
        {
            if (img == null || pictureBox.Image == null)
            {
                pictureBox.ImageIpl = img;
                imageID = img.Width + img.Height + img.Channels() + img.Type().Value;
            }
            else if (img.Width + img.Height + img.Channels() + img.Type().Value != imageID)
            {
                pictureBox.ImageIpl = img;
                imageID = img.Width + img.Height + img.Channels() + img.Type().Value;
            }
            else
            {
                BitmapConverter.ToBitmap(img, (Bitmap)pictureBox.Image);
            }
            pictureBox.Invalidate();
        }

        private RendererMode rendererMode = RendererMode.AspectRatioScale;

        public RendererMode RendererMode
        {
            get { return rendererMode; }
            set {
                rendererMode = value;
                HandleResize();
            }
        }

        public Renderer()
        {
            pictureBox = new PictureBoxIpl();
            BoundsChanged = new Subject<Rectangle>();
            InitializeComponent();
            SetSize(new Rectangle(1200, 50, 512, 512));
            prevHeight = prevWidth = 0;
            Show();
            HandleResize();
        }

        private void Renderer_Load(object sender, EventArgs e)
        {
            pictureBox.ImageIpl = this.Image?.Mat;
            Controls.Add(pictureBox);
        }

        public void Enable(bool enabled)
        {
            this.enabled = enabled;
            Visible = enabled;
        }

        private void HandleResize()
        {
            if (image != null)
            {
                if (image == CvImage.Damon)
                {
                    pictureBox.SizeMode = PictureBoxSizeMode.Normal;
                    if (ClientSize.Width != 512 || ClientSize.Height != 512)
                        ClientSize = pictureBox.ClientSize = new System.Drawing.Size(512, 512);
                    if (rendererMode == RendererMode.SizeFromImage)
                        MaximumSize = MinimumSize = SizeFromClientSize(ClientSize);
                }
                else
                {
                    if (rendererMode == RendererMode.SizeFromImage)
                    {
                        if (ClientSize.Width != image.Width || ClientSize.Height != image.Height)
                        {
                            ClientSize = pictureBox.ClientSize = new System.Drawing.Size(image.Width, image.Height);
                            MaximumSize = MinimumSize = SizeFromClientSize(ClientSize);
                        }
                    }
                    else if (rendererMode == RendererMode.AspectRatioScale)
                    {
                        if (rendererMode == RendererMode.AspectRatioScale)
                        {
                            ClientSize = pictureBox.ClientSize = new System.Drawing.Size(image.Width, image.Height);
                        }
                        MinimumSize = MaximumSize = new System.Drawing.Size(0, 0);
                        pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    else
                    {
                        MinimumSize = MaximumSize = new System.Drawing.Size(0, 0);
                        pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        pictureBox.ClientSize = ClientSize;
                    }
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (rendererMode == RendererMode.AspectRatioScale)
            {
                if (m.Msg == WM_SIZING)
                {
                    RECT rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
                    int res = m.WParam.ToInt32();
                    if (res == WMSZ_LEFT || res == WMSZ_RIGHT)
                    {
                        //Left or right resize -> adjust height (bottom)
                        rc.Bottom = rc.Top + (int)(heightRatio * this.Width / widthRatio);
                    }
                    else if (res == WMSZ_TOP || res == WMSZ_BOTTOM)
                    {
                        //Up or down resize -> adjust width (right)
                        rc.Right = rc.Left + (int)(widthRatio * this.Height / heightRatio);
                    }
                    else if (res == WMSZ_RIGHT + WMSZ_BOTTOM)
                    {
                        //Lower-right corner resize -> adjust height (could have been width)
                        rc.Bottom = rc.Top + (int)(heightRatio * this.Width / widthRatio);
                    }
                    else if (res == WMSZ_LEFT + WMSZ_TOP)
                    {
                        //Upper-left corner -> adjust width (could have been height)
                        rc.Left = rc.Right - (int)(widthRatio * this.Height / heightRatio);
                    }
                    Marshal.StructureToPtr(rc, m.LParam, true);
                }
            }

            base.WndProc(ref m);
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
    }
}
