using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Audio900.Services
{
    /// <summary>
    /// 许可证验证服务
    /// </summary>
    public static class LicenseVerifier
    {
        private const string LicenseFileName = "license.lic";
        private static readonly string LicenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LicenseFileName);
        
        // 密钥必须与LicenseGenerator中的完全一致
        private const string SECRET_KEY = "Audio2025SecretKey!@#$%"; // 必须与生成工具保持一致

        /// <summary>
        /// 验证许可证
        /// </summary>
        /// <returns>验证是否成功</returns>
        public static bool VerifyLicense()
        {
            try
            {
                // 检查许可证文件是否存在
                if (!File.Exists(LicenseFilePath))
                {
                    return false;
                }

                // 读取加密的许可证内容
                string encryptedContent = File.ReadAllText(LicenseFilePath).Trim();
                
                // 解密许可证
                string decryptedMachineCode = Decrypt(encryptedContent);
                
                if (string.IsNullOrEmpty(decryptedMachineCode))
                {
                    return false;
                }

                // 验证机器码
                string currentMachineCode = GetMachineCode();
                if (decryptedMachineCode != currentMachineCode)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取机器码（组合多个硬件特征）
        /// </summary>
        private static string GetMachineCode()
        {
            try
            {
                string cpuId = GetCpuId();
                string mbId = GetBoardSerialNumber();
                string diskId = GetDiskId();
                string macAddr = GetMacAddress();

                // 组合所有硬件信息（与LicenseGenerator保持一致）
                string rawCode = $"{cpuId}|{mbId}|{diskId}|{macAddr}";
                
                // 使用 MD5 生成固定长度的机器码
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(rawCode));
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hash)
                    {
                        sb.Append(b.ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        /// <summary>
        /// 获取CPU ID
        /// </summary>
        private static string GetCpuId()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["ProcessorId"]?.ToString() ?? "Unknown";
                    }
                }
            }
            catch
            {
                return "Unknown";
            }
            return "Unknown";
        }

        /// <summary>
        /// 获取主板序列号
        /// </summary>
        private static string GetBoardSerialNumber()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["SerialNumber"]?.ToString() ?? "Unknown";
                    }
                }
            }
            catch
            {
                return "Unknown";
            }
            return "Unknown";
        }

        /// <summary>
        /// 获取硬盘序列号
        /// </summary>
        private static string GetDiskId()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["SerialNumber"]?.ToString().Trim() ?? "Unknown";
                    }
                }
            }
            catch
            {
                return "Unknown";
            }
            return "Unknown";
        }

        /// <summary>
        /// 获取 MAC 地址
        /// </summary>
        private static string GetMacAddress()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if ((bool)obj["IPEnabled"])
                        {
                            return obj["MacAddress"]?.ToString() ?? "Unknown";
                        }
                    }
                }
            }
            catch
            {
                return "Unknown";
            }
            return "Unknown";
        }


        /// <summary>
        /// 解密字符串
        /// </summary>
        private static string Decrypt(string encryptedText)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(SECRET_KEY.PadRight(32).Substring(0, 32));
                byte[] ivBytes = Encoding.UTF8.GetBytes(SECRET_KEY.PadRight(16).Substring(0, 16));
                
                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(encryptedText)))
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
