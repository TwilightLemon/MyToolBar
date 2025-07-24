using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace MyToolBar.Common.Func;
public static class ImageHelper
{
    #region Image�໥ת����չ����

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);
    public static ImageSource ToImageSource(this Bitmap bitmap)
    {
        IntPtr hBitmap = bitmap.GetHbitmap();
        ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
            hBitmap,
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        if (!DeleteObject(hBitmap))
            throw new System.ComponentModel.Win32Exception();

        return wpfBitmap;

    }
    public static Bitmap ToBitmap(this ImageSource imageSource)
    {
        BitmapSource m = (BitmapSource)imageSource;

        Bitmap bmp = new Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // �ӵ㣺ѡFormat32bppRgb������͸����

        BitmapData data = bmp.LockBits(
        new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

        m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
        bmp.UnlockBits(data);

        return bmp;
    }
    public static BitmapImage ToBitmapImage(this Bitmap bitmap)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            BitmapImage result = new BitmapImage();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }
    }
    public static BitmapImage ToBitmapImage(this byte[] array)
    {
        using (var ms = new MemoryStream(array))
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
    #endregion
    #region ����ģ��ͼ��
    [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int GdipBitmapApplyEffect(IntPtr bitmap, IntPtr effect, ref Rectangle rectOfInterest, bool useAuxData, IntPtr auxData, int auxDataSize);
    /// <summary>
    /// ��ȡ�����˽���ֶε�ֵ����лAaron Lee Murgatroyd
    /// </summary>
    /// <typeparam name="TResult">�ֶε�����</typeparam>
    /// <param name="obj">Ҫ�����л�ȡ�ֶ�ֵ�Ķ���</param>
    /// <param name="fieldName">�ֶε�����.</param>
    /// <returns>�ֶε�ֵ</returns>
    /// <exception cref="System.InvalidOperationException">�޷��ҵ����ֶ�.</exception>
    /// 
    internal static TResult GetPrivateField<TResult>(this object obj, string fieldName)
    {
        if (obj == null) return default(TResult);
        Type ltType = obj.GetType();
        FieldInfo lfiFieldInfo = ltType.GetField(fieldName, System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (lfiFieldInfo != null)
            return (TResult)lfiFieldInfo.GetValue(obj);
        else
            throw new InvalidOperationException(string.Format("Instance field '{0}' could not be located in object of type '{1}'.", fieldName, obj.GetType().FullName));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BlurParameters
    {
        internal float Radius;
        internal bool ExpandEdges;
    }
    [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int GdipCreateEffect(Guid guid, out IntPtr effect);
    private static Guid BlurEffectGuid = new Guid("{633C80A4-1843-482B-9EF2-BE2834C5FDD4}");
    [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int GdipSetEffectParameters(IntPtr effect, IntPtr parameters, uint size);
    public static IntPtr NativeHandle(this Bitmap Bmp)
    {
        // ͨ�������ȡBitmap��˽���ֶ�nativeImage��ֵ����ֵΪGDI+���ڲ�ͼ����
        //�°�Drawing��Nuget�����ֶ��� nativeImage���Ϊ_nativeImage
        return Bmp.GetPrivateField<IntPtr>("_nativeImage");
    }
    [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int GdipDeleteEffect(IntPtr effect);
    public static void GaussianBlur(this Bitmap Bmp, ref Rectangle Rect, float Radius = 10, bool ExpandEdge = false)
    {
        int Result;
        IntPtr BlurEffect;
        BlurParameters BlurPara;
        if ((Radius < 0) || (Radius > 255))
        {
            throw new ArgumentOutOfRangeException("�뾶������[0,255]��Χ��");
        }
        BlurPara.Radius = Radius;
        BlurPara.ExpandEdges = ExpandEdge;
        Result = GdipCreateEffect(BlurEffectGuid, out BlurEffect);
        if (Result == 0)
        {
            IntPtr Handle = Marshal.AllocHGlobal(Marshal.SizeOf(BlurPara));
            Marshal.StructureToPtr(BlurPara, Handle, true);
            GdipSetEffectParameters(BlurEffect, Handle, (uint)Marshal.SizeOf(BlurPara));
            GdipBitmapApplyEffect(Bmp.NativeHandle(), BlurEffect, ref Rect, false, IntPtr.Zero, 0);
            // ʹ��GdipBitmapCreateApplyEffect�������Բ��ı�ԭʼ��ͼ�񣬶���ģ���Ľ��д�뵽һ���µ�ͼ����
            GdipDeleteEffect(BlurEffect);
            Marshal.FreeHGlobal(Handle);
        }
        else
        {
            throw new ExternalException("��֧�ֵ�GDI+�汾������ΪGDI+1.1�����ϰ汾���Ҳ���ϵͳҪ��ΪWin Vista��֮��汾.");
        }
    }

    static double SrgbToLinear(double c) => c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    static double GetRelativeLuminance(int R,int G,int B)
    {
        double r = SrgbToLinear(R / 255.0);
        double g = SrgbToLinear(G / 255.0);
        double b = SrgbToLinear(B / 255.0);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }
    public static bool WSAGColorCheck(int R, int G, int B)
    {
        double bgLum = GetRelativeLuminance(R,G,B);

        double contrastWithWhite = (1.05) / (bgLum + 0.05);
        double contrastWithBlack = (bgLum + 0.05) / 0.05;

        return contrastWithWhite < contrastWithBlack;// TRUE:black text
    }
    #endregion
}