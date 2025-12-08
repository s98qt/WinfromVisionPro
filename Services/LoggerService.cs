using NLog;
using System;

namespace Audio900.Services
{
    /// <summary>
    /// 日志服务 
    /// </summary>
    public class LoggerService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public static void Info(string message)
        {
            _logger.Info(message);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        public static void Debug(string message)
        {
            _logger.Debug(message);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public static void Warn(string message)
        {
            _logger.Warn(message);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public static void Error(Exception ex, string message)
        {
            _logger.Error(ex, message);
        }

        /// <summary>
        /// 记录作业开始
        /// </summary>
        public static void LogWorkflowStart(string templateName, string productSN)
        {
            Info($"【作业开始】模板: {templateName}, 产品SN: {productSN}");
        }

        /// <summary>
        /// 记录作业完成
        /// </summary>
        public static void LogWorkflowComplete(string templateName, string productSN, bool success)
        {
            if (success)
                Info($"【作业完成】模板: {templateName}, 产品SN: {productSN}, 结果: 成功");
            else
                Warn($"【作业完成】模板: {templateName}, 产品SN: {productSN}, 结果: 失败");
        }

        /// <summary>
        /// 记录步骤执行
        /// </summary>
        public static void LogStepExecution(int stepNumber, string stepName, double score, bool passed)
        {
            Info($"【步骤{stepNumber}】{stepName}, 分数: {score:F3}, 结果: {(passed ? "通过" : "失败")}");
        }

        /// <summary>
        /// 记录模板匹配
        /// </summary>
        public static void LogTemplateMatch(int stepNumber, double score, double threshold)
        {
            Debug($"【模板匹配】步骤{stepNumber}, 分数: {score:F3}, 阈值: {threshold:F3}");
        }
    }
}
