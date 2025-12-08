using NLog;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Accord.Video.FFMPEG;
using System.Drawing;
using System.Drawing.Imaging;
using Cognex.VisionPro;

namespace Audio900.Services
{
    /// <summary>
    /// 视频录制服务
    /// </summary>
    public class VideoRecordingService : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        private bool _isRecording;
        private string _currentVideoPath;        // 当前视频文件路径
        private readonly string _videoDirectory;
        private int _frameCount;
        private readonly object _recordLock = new object();
        private DateTime _recordStartTime;
        private string _productSN;
        private string _templateName;
        
        // Accord.Video.FFMPEG 视频写入器
        private VideoFileWriter _videoWriter;
        private int _imageWidth = 1920;
        private int _imageHeight = 1080;
        private int _frameRate = 25;  // 25 fps
        private bool _isFirstFrame = true;

        public bool IsRecording => _isRecording;
        public string CurrentVideoPath => _currentVideoPath;
        public int FrameCount => _frameCount;

        public VideoRecordingService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _videoDirectory = Path.Combine(baseDir, "Video");
            Directory.CreateDirectory(_videoDirectory);
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public bool StartRecording(string productSN, string templateName = "")
        {
            try
            {
                if (_isRecording)
                {
                    _logger.Warn("视频录制已在进行中，无法重复启动");
                    return false;
                }

                _productSN = productSN;
                _templateName = templateName;
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{productSN}_{timestamp}.avi";
                _currentVideoPath = Path.Combine(_videoDirectory, fileName);

                _isRecording = true;
                _frameCount = 0;
                _recordStartTime = DateTime.Now;
                _isFirstFrame = true;

                _logger.Info($"【视频录制开始】SN: {productSN}, 模板: {templateName}, 文件: {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"启动视频录制失败: SN={productSN}");
                return false;
            }
        }

        /// <summary>
        /// 直接写入Bitmap帧
        /// </summary>
        public void WriteFrameDirect(Bitmap bitmap)
        {
            if (!_isRecording || bitmap == null)
                return;

            lock (_recordLock)
            {
                try
                {
                    // 第一帧时创建视频写入器
                    if (_isFirstFrame)
                    {
                        _imageWidth = bitmap.Width;
                        _imageHeight = bitmap.Height;
                        
                        _videoWriter = new VideoFileWriter();
                        _videoWriter.Open(_currentVideoPath, _imageWidth, _imageHeight, _frameRate, VideoCodec.MPEG4);
                        
                        _isFirstFrame = false;
                        _logger.Debug($"视频写入器已创建: {_imageWidth}x{_imageHeight}, {_frameRate}fps");
                    }
                    
                    if(_videoWriter != null)
                    {
                        // 直接写入帧到视频
                        _videoWriter.WriteVideoFrame(bitmap);
                    }
                   
                    _frameCount++;
                    
                    // 每100帧记录一次
                    if (_frameCount % 100 == 0)
                    {
                        _logger.Debug($"视频录制进度: 已录制 {_frameCount} 帧");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"写入视频帧失败: 帧编号={_frameCount}");
                }
            }
        }
        
        /// <summary>
        /// 写入帧
        /// </summary>
        public void WriteFrame(ICogImage image)
        {
            if (!_isRecording || image == null)
                return;

            lock (_recordLock)
            {
                try
                {
                    // 将 ICogImage 转换为 Bitmap
                    Bitmap bitmap = ICogImageToBitmap(image);
                    
                    if (bitmap == null)
                    {
                        _logger.Warn("ICogImage 转 Bitmap 失败，跳过此帧");
                        return;
                    }
                    
                    // 调用直接写入方法
                    WriteFrameDirect(bitmap);
                    bitmap.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"写入视频帧失败: 帧编号={_frameCount}");
                }
            }
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public void StopRecording()
        {
            if (!_isRecording)
                return;

            lock (_recordLock)
            {
                try
                {
                    // 关闭视频写入器
                    if (_videoWriter != null)
                    {
                        _videoWriter.Close();
                        _videoWriter.Dispose();
                        _videoWriter = null;
                    }
                    
                    _isRecording = false;
                    TimeSpan duration = DateTime.Now - _recordStartTime;
                    double avgFps = duration.TotalSeconds > 0 ? _frameCount / duration.TotalSeconds : 0;

                    // 保存录制信息到视频文件同目录
                    string infoPath = Path.ChangeExtension(_currentVideoPath, ".txt");
                    File.WriteAllText(infoPath,
                        $"产品SN: {_productSN}\n" +
                        $"作业模板: {_templateName}\n" +
                        $"开始时间: {_recordStartTime:yyyy-MM-dd HH:mm:ss}\n" +
                        $"结束时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                        $"时长: {duration.TotalSeconds:F1}秒\n" +
                        $"总帧数: {_frameCount}\n" +
                        $"平均帧率: {avgFps:F1} fps\n" +
                        $"视频文件: {_currentVideoPath}");

                    _logger.Info($"【视频录制完成】SN: {_productSN}, 总帧数: {_frameCount}, 时长: {duration.TotalSeconds:F1}秒, 平均帧率: {avgFps:F1} fps, 文件: {Path.GetFileName(_currentVideoPath)}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"停止视频录制失败: SN={_productSN}");
                }
            }
        }
        
        /// <summary>
        /// 将 ICogImage 转换为 Bitmap
        /// </summary>
        private Bitmap ICogImageToBitmap(ICogImage image)
        {
            try
            {
                if (image == null) return null;
                // 使用 VisionPro 的扩展方法直接转换
                return image.ToBitmap();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ICogImage 转 Bitmap 失败");
                return null;
            }
        }

        public void Dispose()
        {
            if (_isRecording)
                StopRecording();
        }
    }
}
