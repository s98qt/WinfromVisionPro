using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Audio900.Tool
{
    class VProTool
    {
        /// <summary>
        /// 将图像转换成CogImage8Grey
        /// </summary>
        /// <param name="imagepath">图片路径</param>
        public static CogImage8Grey Get8GreyImage(string imagePath)
        {
            if (!File.Exists(imagePath))
                return null;
            CogImageFile ImageFile = new CogImageFile();
            CogImage8Grey Image;
            ImageFile.Open(imagePath, CogImageFileModeConstants.Read);
            Image = (CogImage8Grey)ImageFile[0];
            ImageFile.Close();
            return Image;
        }

        /// <summary>
        /// 将图像转换成ICogImage
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        public static ICogImage GetCogImage(string imagePath)
        {
            if (!File.Exists(imagePath))
                return null;
            CogImageFile ImageFile = new CogImageFile();
            ICogImage Image; 
            ImageFile.Open(imagePath, CogImageFileModeConstants.Read);
            Image = ImageFile[0];
            ImageFile.Close();
            return Image;
        }

        /// <summary>
        /// 根据图片路径, 返回一张灰度图
        /// </summary>
        /// <param name="strPicPath">图片路径</param>
        /// <returns>灰度图对象</returns>
        public static Image GetGrayPicture(string strPicPath)
        {
            /*
            * Stride: 图像扫描宽度
            * 图像在内存里是按行存储的。扫描行宽度就是存储一行像素，用了多少字节的内存。
            * 比如一个101×200大小的图像，每个像素是32位的（也就是每个像素4个字节），那么实际每行占用的内存大小是101*4=404个字节。
            * 然后一般的图像库都会有个内存对齐。假设按照8字节对齐，那么404按照8字节对齐的话，实际是408个字节。这408个字节就是扫描行宽度。
            * 
            * Interop: 托管/非托管代码之间的互操作
            * 
            * Marshal: 类提供了一个方法集，这些方法用于分配非托管内存、复制非托管内存块、将托管类型转换为非托管类型，
            * 此外还提供了在与非托管代码交互时使用的其他杂项方法
            */
            // Create a new bitmap.
            Bitmap bitmap = new Bitmap(strPicPath);
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            // Lock the bitmap's bits. 
            //转成24rgb颜色 24色就是由r g b, 三个颜色, 每个颜色各用一字节(8位)表示亮度
            // BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int iStride = bmpData.Stride; //图片一行象素所占用的字节  

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int iBytes = iStride * bitmap.Height;
            byte[] rgbValues = new byte[iBytes];


            // Copy RGB values into the Array

            Marshal.Copy(ptr, rgbValues, 0, iBytes);

            for (int y = 0; y < bmpData.Height; ++y)
            {
                for (int x = 0; x < bmpData.Width; ++x)
                {
                    //图像(x, y)坐标坐标中第1个像素中rgbValues中的索引位置(这儿x,y是从0开始记的)
                    //rgbValues每行是扫描宽个字节, 不是bitmap.Width * 3
                    int iThird = iStride * y + 3 * x;
                    byte avg = (byte)((rgbValues[iThird] * 0.299 + rgbValues[iThird + 1] * 0.587 + rgbValues[iThird + 2] * 0.114));//转化成灰度
                    rgbValues[iThird] = avg;
                    rgbValues[iThird + 1] = avg;
                    rgbValues[iThird + 2] = avg;
                }
            }
            // copy到原来图像中
            Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);

            // Unlock the bits.
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        /// <summary>
        /// 任务_VPro图片存储
        /// </summary>
        /// <param name="image">VPro控件</param>
        /// <param name="filePath">文件路径(带文件名与后缀)</param>
        /// <param name="isRecotd">是否存储结果原图</param>
        public static void SaveVProImage(CogRecordDisplay imageDisplay, string filePath, bool isRecotd)
        {
            if (isRecotd)//存储带结果原图
            {
                //另一种保存record图片方式
                //imageDisplay.Display.CreateContentBitmap(new CogDisplayContentBitmapConstants(), new CogRectangle(), 0).Save(path + @"\" + "testRecord.jpg");
                Bitmap bmp = imageDisplay.CreateContentBitmap(CogDisplayContentBitmapConstants.Image) as Bitmap;
                bmp.Save(filePath);
            }
            else {//存储原图
                imageDisplay.Image.ToBitmap().Save(filePath);
            }
        }

        /// <summary>
        /// 根据旋转中心旋转角度后得出坐标
        /// </summary>
        /// <param name="XRotation">旋转中心X</param>
        /// <param name="YRotation">旋转中心Y</param>
        /// <param name="ARotate">角度</param>
        /// <param name="XBefore">示教点X</param>
        /// <param name="YBefore">示教点Y</param>
        /// <param name="XAfter">旋转后的X</param>
        /// <param name="YAfter">旋转后的Y</param>
        /// <returns></returns>
        public static bool RotateAngle(double XRotation, double YRotation, double ARotate, double XBefore, double YBefore, ref double XAfter, ref double YAfter)
        {
            //角度转弧度
            double Rad = 0;
            Rad = ARotate * Math.Acos(-1) / 180;
            XAfter = (XBefore - XRotation) * Math.Cos(Rad) - (YBefore - YRotation) * Math.Sin(Rad) + XRotation;
            YAfter = (YBefore - YRotation) * Math.Cos(Rad) + (XBefore - XRotation) * Math.Sin(Rad) + YRotation;
            return true;
        }

    }
}
