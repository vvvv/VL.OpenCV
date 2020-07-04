using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.UserInterface;
using System;
using System.Drawing;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        const int WM_SIZING = 0x214;
        const int WMSZ_LEFT = 1;
        const int WMSZ_RIGHT = 2;
        const int WMSZ_TOP = 3;
        const int WMSZ_BOTTOM = 6;

        private RendererMode rendererMode = RendererMode.AspectRatioScale;
        private string title = "VL.OpenCV Renderer";
        private PictureBoxIpl pictureBox;
        private CvImage image;
        private bool showText = false;
        private bool enabled = true;
        private bool loaded = false;
        private int imageID = 0;

        double aspectRatio = 1;
        System.Drawing.Size sizeDelta;
        public Subject<Rectangle> BoundsChanged { get; }

        public CvImage Image
        {
            get { return image; }
            set
            {
                if (enabled && value != image && value != null && (value.Width > 0 && value.Height > 0))
                {
                    loaded = false;
                    image = value;
                    aspectRatio = (double)image.Cols / (double)image.Rows;
                    if (image != null)
                    {
                        if (image.Width + image.Height + image.Channels + image.Mat.Type().Value != imageID)
                            HandleResize();
                        RefreshIplImage(image?.Mat);
                    }
                    loaded = true;
                    AddText();
                }
            }
        }

        public string Title
        {
            set
            {
                Text = title = value;
                sizeDelta = Size - ClientSize;
            }
        }

        public bool ShowText
        {
            set
            {
                showText = value;
                AddText();
            }
        }

        public RendererMode RendererMode
        {
            get { return rendererMode; }
            set
            {
                rendererMode = value;
                switch (rendererMode)
                {
                    case RendererMode.SizeFromImage:
                        FormBorderStyle = FormBorderStyle.FixedSingle;
                        break;
                    case RendererMode.AspectRatioScale:
                        FormBorderStyle = FormBorderStyle.Sizable;
                        pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        break;
                    default:
                        FormBorderStyle = FormBorderStyle.Sizable;
                        pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        break;
                }
                HandleResize();
            }
        }

        public Renderer()
        {
            pictureBox = new PictureBoxIpl();
            BoundsChanged = new Subject<Rectangle>();
            InitializeComponent();
            SetSize(new Rectangle(1200, 50, 512, 512), true);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.ClientSize = ClientSize;
            Show();
            sizeDelta = Size - ClientSize;
            HandleResize();
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

        private void Renderer_Load(object sender, EventArgs e)
        {
            pictureBox.ImageIpl = Image?.Mat;
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
                    pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    if (Size.Width != 613 || Size.Height != 613)
                        ClientSize = new System.Drawing.Size(ClientSize.Width, (int)(ClientSize.Width / aspectRatio));
                    //if (rendererMode == RendererMode.SizeFromImage)
                        //MaximumSize = MinimumSize = SizeFromClientSize(ClientSize);
                }
                else
                {
                    if (rendererMode == RendererMode.SizeFromImage)
                    {
                        if (ClientSize.Width != image.Width || ClientSize.Height != image.Height)
                        {
                            ClientSize = new System.Drawing.Size(image.Width, image.Height);
                        }
                    }
                    else if (rendererMode == RendererMode.AspectRatioScale)
                    {
                        //ClientSize = new System.Drawing.Size(ClientSize.Width, (int)(ClientSize.Width / aspectRatio));
                        ClientSize = new System.Drawing.Size(ClientSize.Width, (int)(ClientSize.Width / aspectRatio));
                        //needed to prevent underscaling when in AspectRatioScale mode
                        MinimumSize = new System.Drawing.Size(2 * sizeDelta.Width, (int)((2 * sizeDelta.Width) / aspectRatio) + sizeDelta.Height);
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
                    if (res == WMSZ_LEFT || res == WMSZ_RIGHT || res == WMSZ_RIGHT + WMSZ_BOTTOM || res == WMSZ_LEFT + WMSZ_BOTTOM)
                    {
                        //Left/Right resize
                        var currentClientWidth = rc.Right - rc.Left - sizeDelta.Width;
                        var targetClientHeight = Math.Round(currentClientWidth / aspectRatio, MidpointRounding.AwayFromZero);
                        rc.Bottom = rc.Top + (int)targetClientHeight + sizeDelta.Height;
                    }
                    else if (res == WMSZ_TOP || res == WMSZ_BOTTOM || res == WMSZ_RIGHT + WMSZ_TOP)
                    {
                        //Top/Bottom/Top-Right resize
                        var currentClientHeight = rc.Bottom - rc.Top - sizeDelta.Height;
                        var targetClientWidth = Math.Round(currentClientHeight * aspectRatio, MidpointRounding.AwayFromZero);
                        rc.Right = rc.Left + (int)targetClientWidth + sizeDelta.Width;
                    }
                    else if (res == WMSZ_LEFT + WMSZ_TOP)
                    {
                        //Top-Left resize
                        var currentClientHeight = rc.Bottom - rc.Top - sizeDelta.Height;
                        var targetClientWidth = Math.Round(currentClientHeight * aspectRatio, MidpointRounding.AwayFromZero);
                        rc.Left = rc.Right - (int)targetClientWidth - sizeDelta.Width;
                    }
                    Marshal.StructureToPtr(rc, m.LParam, true);
                }
            }
            base.WndProc(ref m);
        }

        public void SetSize(Rectangle bounds, bool apply)
        {
            if (apply)
            {
                var boundsinPix = DIPHelpers.DIPToPixel(bounds);
                if (boundsinPix != Bounds)
                    Bounds = boundsinPix;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            BoundsChanged.OnNext(DIPHelpers.DIP(Bounds));
            if (loaded)
                Text = "cw: " + ClientSize.Width + "   ch: " + ClientSize.Height;
            pictureBox.ClientSize = ClientSize;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Text = title;
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            BoundsChanged.OnNext(DIPHelpers.DIP(Bounds));
        }
    }
}
