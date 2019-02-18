using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.UserInterface;
using System;
using System.Drawing;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VL.Core.Properties;

namespace VL.OpenCV
{
    public enum RendererMode
    {
        AspectRatioScale,
        FreeTransform,
        SizeFromImage
    }

    public partial class Renderer : Form, IDisposable
    {
        //double so division keeps decimal points
        double widthRatio = 1;
        double heightRatio = 1;
        double aspectRatio = 1;

        System.Drawing.Size sizeDelta;

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
        private bool enabled = true;
        private int imageID = 0;
        private bool showText = false;
        private string title = "VL.OpenCV Renderer";
        private bool loaded = false;

        public CvImage Image
        {
            get { return image; }
            set
            {
                if (enabled && value != image)
                {
                    loaded = false;
                    image = value;
                    widthRatio = image.Cols;
                    heightRatio = image.Cols;
                    aspectRatio = widthRatio / image.Rows;
                    if (image != null)
                    {
                        if (rendererMode == RendererMode.AspectRatioScale &&
                            image.Width + image.Height + image.Channels + image.Mat.Type().Value != imageID)
                        {
                            ClientSize = new System.Drawing.Size(ClientSize.Width, (int)(ClientSize.Width / aspectRatio));
                            MinimumSize = new System.Drawing.Size(122 + sizeDelta.Width, (int)((122 + sizeDelta.Width) / aspectRatio) + sizeDelta.Height);
                        }
                        RefreshIplImage(image?.Mat);
                        HandleResize();
                    }
                    loaded = true;
                }
            }
        }

        public string Title
        {
            set
            {
                this.Text = title = value;
                sizeDelta = new System.Drawing.Size(SystemInformation.BorderSize.Width * 2,
                    SystemInformation.CaptionHeight);
            }
        }

        public bool ShowText
        {
            set {
                showText = value;
                AddText();
            }
        }

        private void AddText()
        {
            if (showText)
            {
                var info = image.Width + "x" + image.Height + "x" + image.Channels;
                InfoLabel.Text = info;
            }
            InfoLabel.Visible = showText;
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
            this.BackColor = Color.Blue;
            pictureBox.BackColor = Color.Transparent;
            BoundsChanged = new Subject<Rectangle>();
            InitializeComponent();
            SetSize(new Rectangle(1200, 50, 512, 512));
            Show();
            sizeDelta = new System.Drawing.Size(SystemInformation.BorderSize.Width * 2,
                    SystemInformation.CaptionHeight);
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
                    if (Size.Width != 613 || Size.Height != 613)
                        ClientSize = pictureBox.ClientSize = new System.Drawing.Size(ClientSize.Width, (int)(ClientSize.Width / aspectRatio));
                    if (rendererMode == RendererMode.SizeFromImage)
                        MaximumSize = MinimumSize = SizeFromClientSize(ClientSize);
                }
                else
                {
                    if (rendererMode == RendererMode.SizeFromImage)
                    {
                        if (ClientSize.Width != image.Width || ClientSize.Height != image.Height)
                        {
                            ClientSize = new System.Drawing.Size(image.Width, image.Height);
                            FormBorderStyle = FormBorderStyle.FixedSingle;
                        }
                    }
                    else if (rendererMode == RendererMode.AspectRatioScale)
                    {
                        FormBorderStyle = FormBorderStyle.Sizable;
                        pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    else
                    {
                        FormBorderStyle = FormBorderStyle.Sizable;
                        pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
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
                        rc.Bottom = rc.Top + (int)((rc.Right - rc.Left) / aspectRatio) + sizeDelta.Height;
                    }
                    else if (res == WMSZ_TOP || res == WMSZ_BOTTOM || res == WMSZ_RIGHT + WMSZ_TOP)
                    {
                        //Up or down resize -> adjust width (right)
                        rc.Right = rc.Left + (int)((rc.Bottom - rc.Top) * aspectRatio) - sizeDelta.Height;
                    }
                    else if (res == WMSZ_RIGHT + WMSZ_BOTTOM || res == WMSZ_LEFT + WMSZ_BOTTOM)
                    {
                        //Lower-right/Lower-left corner resize -> adjust height (could have been width)
                        rc.Bottom = rc.Top + (int)((rc.Right - rc.Left) / aspectRatio) + sizeDelta.Height;
                    }
                    else if (res == WMSZ_LEFT + WMSZ_TOP)
                    {
                        //Upper-left corner -> adjust width (could have been height)
                        rc.Left = rc.Right - (int)((rc.Bottom - rc.Top) * aspectRatio) + sizeDelta.Height;
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
            if (loaded) 
                Text = "cw: " + pictureBox.ClientSize.Width + "   ch: " + pictureBox.ClientSize.Height;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Text = title;
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            BoundsChanged.OnNext(Settings.DIP(Bounds));
        }
    }
}
