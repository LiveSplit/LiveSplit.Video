using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LiveSplit.Video
{
    public class GDI32
    {
        [DllImport("Gdi32.dll")]
        public static extern int BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
                IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        public enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020
        };

        public static void DrawControlToBitMap(Control srcControl, Bitmap destBitmap, Rectangle destBounds)
        {
            try
            {
                using (Graphics srcGraph = srcControl.CreateGraphics())
                {
                    IntPtr srcHdc = srcGraph.GetHdc();
                    User32.SendMessage(srcControl.Handle, User32.WM.WM_PRINT, (int)srcHdc, 30);

                    using (Graphics destGraph = Graphics.FromImage(destBitmap))
                    {
                        IntPtr destHdc = destGraph.GetHdc();
                        BitBlt(destHdc, destBounds.X, destBounds.Y, destBounds.Width, destBounds.Height,
                            srcHdc, 0, 0, TernaryRasterOperations.SRCCOPY);
                        destGraph.ReleaseHdc(destHdc);
                    }

                    srcGraph.ReleaseHdc(srcHdc);
                }
            }
            catch { }
        }
    }

    public static partial class User32
    {
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, WM Msg, int wParam, int lParam);
        public enum WM
        {
            WM_PRINT = 0x0317
        }
    }
}
