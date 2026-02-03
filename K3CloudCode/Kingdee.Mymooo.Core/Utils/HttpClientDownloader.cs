using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Utils
{
    public class HttpClientDownloader
    {
        private readonly HttpClient _httpClient;

        public HttpClientDownloader()
        {
            _httpClient = new HttpClient();
            // 设置超时
            _httpClient.Timeout = TimeSpan.FromMinutes(30);
            // 可选：设置默认请求头
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        public async Task DownloadFileAsync(string url, string localFilePath)
        {
            try
            {
                using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await contentStream.CopyToAsync(fileStream);
                        //Console.WriteLine($"文件已下载到: {localFilePath}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP请求错误: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"请求超时或取消: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"下载失败: {ex.Message}");
            }
        }
    }

    public class HttpWebRequestDownloader
    {
        public void DownloadFile(string url, string localFilePath)
        {
            HttpWebRequest request = null;
            WebResponse response = null;
            Stream responseStream = null;
            FileStream fileStream = null;

            try
            {
                // 确保目录存在
                var directory = Path.GetDirectoryName(localFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                // 创建请求
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 300000; // 5分钟超时
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
                // 获取响应（同步操作）
                response = request.GetResponse();
                responseStream = response.GetResponseStream();
                // 创建文件流
                fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                byte[] buffer = new byte[4096];
                int bytesRead;
                long totalBytesRead = 0;
                long totalBytes = response.ContentLength;
                // 读取数据并写入文件（同步操作）
                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    //// 显示进度
                    //if (totalBytes > 0)
                    //{
                    //    double progress = (double)totalBytesRead / totalBytes * 100;
                    //    Console.Write($"\r下载进度: {progress:F2}% ({totalBytesRead / 1024} KB / {totalBytes / 1024} KB)");
                    //}
                }

                //Console.WriteLine($"\n文件已下载到: {localFilePath}");
                //Console.WriteLine($"文件大小: {totalBytesRead} 字节");
            }
            catch (WebException ex)
            {
                // 删除可能已部分下载的文件
                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                }
                if (ex.Response is HttpWebResponse httpResponse)
                {
                    //Console.WriteLine($"HTTP错误 ({httpResponse.StatusCode}): {httpResponse.StatusDescription}");
                    throw new Exception($"HTTP错误 ({httpResponse.StatusCode}): {httpResponse.StatusDescription}");
                }
                else
                {
                    //Console.WriteLine($"网络错误: {ex.Message}");
                    throw new Exception($"网络错误: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // 删除可能已部分下载的文件
                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                }

                throw new Exception($"下载失败: {ex.Message}");
            }
            finally
            {
                // 确保所有资源都被正确释放
                fileStream?.Close();
                responseStream?.Close();
                response?.Close();
            }
        }
    }
}

