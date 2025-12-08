using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio900.Models
{
    public class ShapeModel
    {
        private static object syncObj = new object();
        private static ShapeModel _instance;
        public static ShapeModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncObj)
                    {
                        if (_instance == null)
                            _instance = new ShapeModel();
                    }

                }

                return _instance;
            }
        }

        [Category("橡皮擦"), Description("橡皮擦半径大小")]
        public double Size { get; set; } = 6;

        /// <summary>
        ///  创建模板参数
        /// </summary>
        [Category("创建模板参数"), Description("金字塔层数*[定义数组][开始层和结束层][可以达到多层金字塔搜索]")]
        public int NumLevels_CreateModel { get; set; } = 2;
        [Category("创建模板参数"), Description("开始角度（弧度）")]
        public double AngleStart_CreateModel { get; set; } =-3.14;
        [Category("创建模板参数"), Description("角度范围（弧度）")]
        public double AngleExtent_CreateModel { get; set; } =6.28;
        [Category("创建模板参数"), Description("角度步长,设定值越小，程序耗时越长")]
        public string AngleStep_CreateModel { get; set; } = "auto";
        [Category("创建模板参数"), Description("最小缩放")]
        public double ScaleMin_CreateModel { get; set; } = 0.7;
        [Category("创建模板参数"), Description("最大缩放")]
        public double ScaleMax_CreateModel { get; set; } = 1.3;
        [Category("创建模板参数"), Description("缩放步长")]
        public string ScaleStep_CreateModel { get; set; } = "auto";
        [Category("创建模板参数"), Description("优化方式")]
        public string Optimization_CreateModel { get; set; } = "auto";
        [Category("创建模板参数"), Description("极性控制:\r\n  use_polarity - 严格极性（黑白对比度一致）\r\n  ignore_global_polarity - 忽略极性（推荐，适应光照变化）\r\n  ignore_local_polarity - 忽略局部极性")]
        public string Metric_CreateModel { get; set; } = "ignore_global_polarity";  // 忽略极性
        
        [Category("创建模板参数"), Description("对比度阈值（降低可提取更多特征，推荐 10-20）")]
        public int Contrast_CreateModel { get; set; } = 15;
        
        [Category("创建模板参数"), Description("最小对比度（auto 或 5-10）")]
        public string MinContrast_CreateModel { get; set; } = "auto";


        /// <summary>
        /// 模板匹配参数
        /// </summary>
        [Category("模板匹配参数"), Description("开始角度")]
        public double AngleStart_FindModel { get; set; } = -3.14;
        [Category("模板匹配参数"), Description("角度范围")]
        public double AngleExtent_FindModel { get; set; } = 6.28;
        [Category("模板匹配参数"), Description("最小缩放")]
        public double ScaleMin_FindModel { get; set; } = 0.7;
        [Category("模板匹配参数"), Description("最大缩放")]
        public double ScaleMax_FindModel { get; set; } = 1.3;
        [Category("模板匹配参数"), Description("最小得分（降低可提高检出率，推荐 0.3-0.4）")]
        public double MinScore_FindModel { get; set; } = 0.35; 
        
        [Category("模板匹配参数"), Description("匹配个数")]
        public int NumMatches_FindModel { get; set; } = 1;
        
        [Category("模板匹配参数"), Description("最大重叠")]
        public double MaxOverlap_FindModel { get; set; } = 0.5;
        
        [Category("模板匹配参数"), Description("是否亚像素（least_squares 最精确）")]
        public string SubPixel_FindModel { get; set; } = "least_squares";
        
        [Category("模板匹配参数"), Description("金字塔层数")]
        public double NnumLevels_FindModel { get; set; } = 2;
        
        [Category("模板匹配参数"), Description("贪婪度（值越小搜索越全面，推荐 0.7-0.8）")]
        public double Greediness_FindModel { get; set; } = 0.5;


    }
}
