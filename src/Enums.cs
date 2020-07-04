namespace VL.OpenCV
{
    public enum SobelKSize
    {
        One = 1,
        Three = 3,
        Five = 5,
        Seven = 7
    }

    /// <summary>
    /// Color conversion codes which available in the "from" section of OpenCvSharp.ColorConversionCodes
    /// </summary>
    public enum ColorConversionSourceCodes
    {
        BayerBG,
        BayerGB,
        BayerGR,
        BayerRG,
        BGR,
        BGR555,
        BGR565,
        BGRA,
        COLORCVT_MAX,
        GRAY,
        HLS,
        HSV,
        Lab,
        LBGR,
        LRGB,
        Luv,
        mRGBA,
        RGB,
        RGBA,
        XYZ,
        YCrCb,
        YUV,
        YUV420p,
        YUV420sp
    }

    /// <summary>
    /// Color conversion codes which available in the "to" section of OpenCvSharp.ColorConversionCodes
    /// </summary>
    public enum ColorConversionTargetCodes
    {
        BGR,
        BGR555,
        BGR565,
        BGR_EA,
        BGR_FULL,
        BGR_I420,
        BGR_IYUV,
        BGR_NV12,
        BGR_NV21,
        BGR_UYNV,
        BGR_UYVY,
        BGR_VNG,
        BGR_Y422,
        BGR_YUNV,
        BGR_YUY2,
        BGR_YUYV,
        BGR_YV12,
        BGR_YVYU,
        BGRA,
        BGRA_I420,
        BGRA_IYUV,
        BGRA_NV12,
        BGRA_NV21,
        BGRA_UYNV,
        BGRA_UYVY,
        BGRA_Y422,
        BGRA_YUNV,
        BGRA_YUY2,
        BGRA_YUYV,
        BGRA_YV12,
        BGRA_YVYU,
        COLORCVT_MAX,
        GRAY,
        GRAY_420,
        GRAY_I420,
        GRAY_IYUV,
        GRAY_NV12,
        GRAY_NV21,
        GRAY_UYNV,
        GRAY_UYVY,
        GRAY_Y422,
        GRAY_YUNV,
        GRAY_YUY2,
        GRAY_YUYV,
        GRAY_YV12,
        GRAY_YVYU,
        HLS,
        HLS_FULL,
        HSV,
        HSV_FULL,
        Lab,
        LBGR,
        LRGB,
        Luv,
        mRGBA,
        RGB,
        RGB_EA,
        RGB_FULL,
        RGB_I420,
        RGB_IYUV,
        RGB_NV12,
        RGB_NV21,
        RGB_UYNV,
        RGB_UYVY,
        RGB_VNG,
        RGB_Y422,
        RGB_YUNV,
        RGB_YUY2,
        RGB_YUYV,
        RGB_YV12,
        RGB_YVYU,
        RGBA,
        RGBA_I420,
        RGBA_IYUV,
        RGBA_NV12,
        RGBA_NV21,
        RGBA_UYNV,
        RGBA_UYVY,
        RGBA_Y422,
        RGBA_YUNV,
        RGBA_YUY2,
        RGBA_YUYV,
        RGBA_YV12,
        RGBA_YVYU,
        XYZ,
        YCrCb,
        YUV,
        YUV_I420,
        YUV_IYUV,
        YUV_YV12
    }

    public enum BoardCornersAxes
    {
        XY,
        XZ,
        ZY
    }

    public enum PedestrianDetectorType
    {
        Default,
        Daimler
    }

    public enum MatType
    {
        CV_8UC1,
        CV_8UC2,
        CV_8UC3,
        CV_8UC4,
        CV_8SC1,
        CV_8SC2,
        CV_8SC3,
        CV_8SC4,
        CV_16UC1,
        CV_16UC2,
        CV_16UC3,
        CV_16UC4,
        CV_16SC1,
        CV_16SC2,
        CV_16SC3,
        CV_16SC4,
        CV_32SC1,
        CV_32SC2,
        CV_32SC3,
        CV_32SC4,
        CV_32FC1,
        CV_32FC2,
        CV_32FC3,
        CV_32FC4,
        CV_64FC1,
        CV_64FC2,
        CV_64FC3,
        CV_64FC4,
        Unknown
    }
}
