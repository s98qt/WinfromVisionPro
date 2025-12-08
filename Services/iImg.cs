using System;
using System.Runtime.InteropServices;

namespace iMG
{
    // iImg wrapper class
    public class iImg
    {
        public enum ResizeMethod
        {
            Auto = 0,
            NN = 10,
            Bilinear = 20,
            Area = 30,
            Lanczos = 40,
        }

        [DllImport("iImg.dll", EntryPoint = "iImgLoad")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern public bool Load(string fileName, byte[] data, out int w, out int h, out int bpp);
        [DllImport("iImg.dll", EntryPoint = "iImgSave")]
        static extern public void Save(string fileName, byte[] data, int w, int h, int bpp);

        [DllImport("iImg.dll", EntryPoint = "iImgAdaptBpp")]
        static extern public void AdaptBpp(byte[] dataIn, int w, int h, int bpp, byte[] dataOut, int newBpp);
        [DllImport("iImg.dll", EntryPoint = "iImgGamma")]
        static extern public void Gamma(byte[] data, int w, int h, int bpp, double gamma);
        [DllImport("iImg.dll", EntryPoint = "iImgSaturation")]
        static extern public void Saturation(byte[] data, int w, int h, int bpp, double saturation);

        [DllImport("iImg.dll", EntryPoint = "iImgGetLevelStats")]
        static extern public void GetLevelStats(byte[] data, int w, int h, int bpp, int[] stat, int[] statR, int[] statG, int[] statB);
        [DllImport("iImg.dll", EntryPoint = "iImgGetAutoLevelParams")]
        static extern public void GetAutoLevelParams(int[] lvlStat, out double param1, out double param2);
        [DllImport("iImg.dll", EntryPoint = "iImgLevel")]
        static extern public void Level(byte[] data, int w, int h, int bpp, double[] lvlParams);

        [DllImport("iImg.dll", EntryPoint = "iImgCrop")]
        static extern public void Crop(byte[] dataIn, int w, int h, int bpp, byte[] dataOut, int cropLeft, int cropTop, int cropW, int cropH);
        [DllImport("iImg.dll", EntryPoint = "iImgResize")]
        static extern public void Resize(byte[] dataIn, int w, int h, int bpp, byte[] dataOut, int newW, int newH, ResizeMethod method);
        [DllImport("iImg.dll", EntryPoint = "iImgFlip")]
        static extern public void Flip(byte[] dataIn, int w, int h, int bpp, byte[] dataOut, bool horz, bool vert);
        [DllImport("iImg.dll", EntryPoint = "iImgRotate")]
        static extern public void Rotate(byte[] dataIn, int w, int h, int bpp, byte[] dataOut, float angle);
    }
}
