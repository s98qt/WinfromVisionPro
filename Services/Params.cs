using Audio900.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Params_OUMIT_
{
    public class Params
    {
        private static object syncObj = new object();
        private static Params _instance;
        public static Params Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncObj)
                    {
                        if (_instance == null)
                            _instance = new Params();
                    }
                }

                return _instance;
            }
        }

        //-------------------------功能启用---------------------------------------------

        [DisplayName("是否开启读码选项"), Category("功能选项"), Description("True,则开启读码")]
        public string  isReadCodeEnable { get; set; } 

        [DisplayName("是否开启上传PDCA选项"), Category("功能选项"), Description("True,则开启上传PDCA")]
        public string isPDCAEnable { get; set; }

       
        [DisplayName("是否启用UC获取SN"), Category("功能选项"), Description("True,则启用UC获取SN")]
        public bool isUCgetSN { get; set; }

        [DisplayName("照片数量"), Category("功能选项"), Description("保存图像时长")]
        public int photoNumber { get; set; } = 1;

        [DisplayName("是否开启上传MES选项"), Category("功能选项"), Description("True,则开启上传MES")]
        public string isMESEnable { get; set; }

        [DisplayName("是否开启上传HIVE选项"), Category("功能选项"), Description("True,则开启上传HIVE")]
        public string isHIVEEnable { get; set; }

        [DisplayName("MES回馈超时时间"), Category("功能选项"), Description("上传MES逾时时间，ms")]
        public int MESReturnDelay { get; set; } = 3000;

        [DisplayName("PDCA回馈超时时间"), Category("功能选项"), Description("上传PDCA逾时时间，ms")]
        public int PDCAReturnDelay { get; set; } = 3000;

        [DisplayName("压缩图像超时时间"), Category("功能选项"), Description("压缩图像逾时时间，ms")]
        public int ImageZIPDelay { get; set; } = 3000;

        [DisplayName("保存图像时长"), Category("功能选项"), Description("保存图像时长")]
        public int ImageTime { get; set; } = 6;
        //-------------------------上传信息---------------------------------------------

        [DisplayName("Ping MES IP"), Category("MES信息"), Description("检查MES IP")]
        public string PingMESIP { get; set; }

        [DisplayName("Ping MES Port"), Category("MES信息"), Description("检查MES Port")]
        public int  PingMESPort { get; set; }

        [DisplayName("Ping HIVE IP"), Category("MES信息"), Description("检查HIVE IP")]
        public string PingHIVEIP { get; set; }

        [DisplayName("Ping HIVE Port"), Category("MES信息"), Description("检查HIVE Port")]
        public int  PingHIVEPort { get; set; }

        [DisplayName("上传图片URL"), Category("MES信息"), Description("检查路由的URL")]
        public string upPicURL { get; set; }

        [DisplayName("过站URL"), Category("MES信息"), Description("检查路由的URL")]
        public string url { get; set; }

        [DisplayName("获取SN的URL"), Category("MES信息"), Description("获取令牌的URL")]
        public string getHwdURL { get; set; }

        [DisplayName("获取MES数据的URL"), Category("MES信息"), Description("获取MES数据的URL")]
        public string getMESDataURL { get; set; }

        [DisplayName("上传MES测试数据的URL"), Category("MES信息"), Description("获取MES数据的URL")]
        public string getMESTestDataURL { get; set; }

        [DisplayName("链接PDCA或者MES的IP"), Category("MES信息"), Description("IP地址")]
        public string IP { get; set; }

        [DisplayName("链接PDCA或者MES的端口Port"), Category("MES信息"), Description("端口地址")]
        public string Port { get; set; }

        [DisplayName("OP的账号"), Category("MES信息"), Description("OP的账号")]
        public string OPID { get; set; }

        [DisplayName("OP的密码"), Category("MES信息"), Description("OP的密码")]
        public string OPPassWord { get; set; }

        [DisplayName("站别名"), Category("MES信息"), Description("站别名")]
        public string MachineStation { get; set; }

        [DisplayName("empNo"), Category("作业员工号"), Description("必须传")]
        public string empNo { get; set; }

        [DisplayName("terminalName"), Category("工作站名称"), Description("必须传")]
        public string terminalName { get; set; }

        [DisplayName("serial_Number"), Category("SN"), Description("必须传")]
        public string serial_Number { get; set; }

        /// <summary>
        /// 产品条码
        /// </summary>
        public string SN  { get; set; }

        [DisplayName("线体"), Category("MES信息"), Description("线体")]
        public string lineNo { get; set; }

        [DisplayName("工位"), Category("MES信息"), Description("工位")]
        public string section { get; set; }

        [DisplayName("工站"), Category("MES信息"), Description("工站")]
        public string station { get; set; }

        [DisplayName("项目号"), Category("MES信息"), Description("项目号")]
        public string ProjectCode { get; set; }

        [DisplayName("本地暂存图片路径"), Category("MES信息"), Description("本地暂存图片路径")]
        public string LocationPicPath { get; set; }

        [DisplayName("本地压缩图片全路径"), Category("MES信息"), Description("本地压缩图片全路径")]
        public string LocationZIPPath { get; set; }
       
        //-------------------------设备信息---------------------------------------------

        [DisplayName("电脑的账号"), Category("设备信息"), Description("电脑的账号")]
        public string user { get; set; }

        [DisplayName("电脑的密码"), Category("设备信息"), Description("电脑的密码")]
        public string passWord { get; set; }

        [DisplayName("设备编号"), Category("设备信息"), Description("设备编号")]
        public string MachineNO { get; set; }

        [DisplayName("软件版本"), Category("设备信息"), Description("软件版本")]
        public string MachineVer { get; set; }

        [DisplayName("显微镜设备ID"), Category("设备信息"), Description("显微镜设备ID")]
        public int MicroID { get; set; }

        [DisplayName("IO卡COM口"), Category("设备信息"), Description("IO卡COM口")]
        public string IOCardCom { get; set; }


        //————————————相机参数————————————————————

        [DisplayName("曝光"), Category("相机信息"), Description("曝光")]
        public string Exposure { get; set; }

        [DisplayName("增益"), Category("相机信息"), Description("增益")]
        public string Gain { get; set; }

        [Browsable(false)]
        public privateParams privateParams { get; set; } = new privateParams();
       
        public CamerLine camerLine { get; set; } = new CamerLine();
    }
    public class privateParams
    {
        [DisplayName("用于发给PDCA的起始时间"), Category("MES信息"), Description("存储发给PDCA的图片的路径")]
        public string startTime { get; set; }= DateTime.Now.ToString();

        [DisplayName("存储发送图片的路径"), Category("MES信息"), Description("存储发送图片的路径")]
        public string ImagePath { get; set; } = @"D:\Image\";

        [DisplayName("工程师权限密码"), Category("设备信息"), Description("工程师权限密码")]
        public string EnginnerPassword { get; set; } = "cowain";

        [DisplayName("管理员权限密码"), Category("设备信息"), Description("管理员权限密码")]
        public string AdminPassword { get; set; } = "cowain";
    }

    /// <summary>
    ///  显示图像时显示2根mark线
    /// </summary>
    public class CamerLine
    {
        public int line1Y { get; set; } = 10;
        public int line2Y { get; set; } = 5;

        public int line3Y { get; set; } = 10;
        public int line4Y { get; set; } = 5;
    }
  
    /// <summary>
    ///  不良代码字典
    /// </summary>
    public class DysNumBer
    {
        [Category("1 不良代码"), DisplayName("不良代码"), Description("")]
        public string dysNumber { get; set; } = "";
        [Category("2 结果"), DisplayName("true为PASS，false为FAIL"), Description("true为PASS，false为FAIL")]
        public bool pass { get; set; } = false;
    }
}
