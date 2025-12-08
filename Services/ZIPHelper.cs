using Audio.Services;
using ICSharpCode.SharpZipLib.Zip;
using Params_OUMIT_;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZIPHelper
{
    public static class myZIPHelper
    {
       
        public static string ZIP(List<string> photoPaths, string ZIPPath, string sn)
        {
            string currentTime = DateTime.Now.ToString("yyyyMMddhhmmss");
            string[] arr = Params.Instance.MachineStation.Split('_');
            string returnPath = ZIPPath + "\\" + sn + "_" + Params.Instance.ProjectCode+"_" +arr[0]+"_"+arr[1]+"_"+ Params.Instance.station+"_"+arr[2]+"_"+ currentTime + ".zip";
            ZipFiles(photoPaths, returnPath, 1, "", "RH650");
           // string returnStr=day+"/"+sn + "_" + currentTime + ".zip";
            return returnPath;
        }
        
        /// <summary>
        /// 制作压缩包（多个文件压缩到一个压缩包，支持加密、注释）
        /// </summary>
        /// <param name="fileNames">要压缩的文件</param>
        /// <param name="topDirectoryName">压缩文件目录</param>
        /// <param name="zipedFileName">压缩包文件名</param>
        /// <param name="compresssionLevel">压缩级别 1-9</param>
        /// <param name="password">密码</param>
        /// <param name="comment">注释</param>
        private static void ZipFiles(List<string> fileNames, string zipedFileName, int? compresssionLevel, string password = "", string comment = "")
        {
            using (ZipOutputStream zos = new ZipOutputStream(File.Open(zipedFileName, FileMode.OpenOrCreate)))
            {
                if (compresssionLevel.HasValue)
                {
                    zos.SetLevel(compresssionLevel.Value);//设置压缩级别
                }

                if (!string.IsNullOrEmpty(password))
                {
                    zos.Password = password;//设置zip包加密密码
                }

                if (!string.IsNullOrEmpty(comment))
                {
                    zos.SetComment(comment);//设置zip包的注释
                }

                foreach (string file in fileNames)
                {
                    // string fileName = string.Format("{0}/{1}", topDirectoryName, file);
                    if (File.Exists(file))
                    {
                        FileInfo item = new FileInfo(file);
                        FileStream fs = File.OpenRead(item.FullName);
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        fs.Close();
                        fs.Dispose();
                        ZipEntry entry = new ZipEntry(item.Name);
                        zos.PutNextEntry(entry);
                        zos.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
        public static void CopyImg(string SN)
        {
            //复制NG图片
            try
            {
                string date = DateTime.Now.ToString("yyyyMMdd");
                string photoPath = @"F:\CognexImage" + "\\" + date + "\\" + SN;
                string[] paths = Directory.GetFiles(photoPath);
                string destpath = @"F:\CognexImage" + "\\" + date + "\\" + "cognex_NG图片" + "\\" + SN;// @"E:\img" + "\\" + SN + ".jpg";
                if (!Directory.Exists(destpath))
                {
                    Directory.CreateDirectory(destpath);
                }
                foreach (string item in paths)
                {
                    if (item.Contains(SN) && item.Contains(".jpg"))
                    {
                        string path = item.Substring(item.LastIndexOf('\\'));
                        File.Copy(item, destpath + path);
                    }
                }
            }
            catch
            { }
        }
        public static bool ZIP(string photoPath, string ZIPFileFullPath, ref string errorMSG)
        {
            try
            {
                string currentTime = DateTime.Now.ToString("yyyyMMdd");

                string dirPath = ZIPFileFullPath.Substring(0, ZIPFileFullPath.LastIndexOf("\\"));

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                //string[] paths = { photoPath };
                string[] paths = null;
                paths = Directory.GetFiles(photoPath);
                ZipFiles(paths, ZIPFileFullPath, 1, "", currentTime);
                //DirectoryInfo subdir = new DirectoryInfo(photoPath);
                //subdir.Delete(true);
            }
            catch (Exception e)
            {
                errorMSG = e.Message;
                return false;
            }
            return true;
        }
        /// <summary>
        /// 制作压缩包（多个文件压缩到一个压缩包，支持加密、注释）
        /// </summary>
        /// <param name="fileNames">要压缩的文件</param>
        /// <param name="topDirectoryName">压缩文件目录</param>
        /// <param name="zipedFileName">压缩包文件名</param>
        /// <param name="compresssionLevel">压缩级别 1-9</param>
        /// <param name="password">密码</param>
        /// <param name="comment">注释</param>
        private static void ZipFiles(string[] fileNames, string zipedFileName, int? compresssionLevel, string password = "", string comment = "")
        {

            FileStream tempSteam = File.Create(zipedFileName);
            using (ZipOutputStream zos = new ZipOutputStream(tempSteam))
            {
                if (compresssionLevel.HasValue)
                {
                    zos.SetLevel(compresssionLevel.Value);//设置压缩级别
                }

                if (!string.IsNullOrEmpty(password))
                {
                    zos.Password = password;//设置zip包加密密码
                }

                if (!string.IsNullOrEmpty(comment))
                {
                    zos.SetComment(comment);//设置zip包的注释
                }

                foreach (string file in fileNames)
                {
                    // string fileName = string.Format("{0}/{1}", topDirectoryName, file);
                    if (File.Exists(file))
                    {
                        FileInfo item = new FileInfo(file);
                        FileStream fs = File.OpenRead(item.FullName);
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        fs.Close();
                        fs.Dispose();
                        ZipEntry entry = new ZipEntry(item.Name);
                        zos.PutNextEntry(entry);
                        zos.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

    }
}
