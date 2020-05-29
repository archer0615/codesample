using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SharpZipHelperHelpers
{
    public static class SharpZipHelper
    {
        /// <summary>
        /// 解壓縮檔案
        /// </summary>
        /// <param name="InputStream">檔案Stream</param>
        /// <param name="targetPath">儲存位置</param>
        public static void UncompressToFile(Stream InputStream, string targetPath)
        {
            int bufferSize = 4096;     //指定的緩衝區大小
            byte[] bufferArray = new byte[bufferSize];
            string fileName;

            ZipEntry theEntry = null;
            FileStream streamWriter = null;

            using (ZipInputStream unzip = new ZipInputStream(InputStream))
            {
                while ((theEntry = unzip.GetNextEntry()) != null)
                {
                    if (theEntry.Name != string.Empty)
                    {
                        fileName = Path.Combine(targetPath, theEntry.Name);
                        //判断文件路径是否是文件夹  
                        if (fileName.EndsWith("/") || fileName.EndsWith("\\"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }

                        streamWriter = System.IO.File.Create(fileName);
                        while (true)
                        {
                            bufferSize = unzip.Read(bufferArray, 0, bufferArray.Length);
                            if (bufferSize > 0)
                            {
                                streamWriter.Write(bufferArray, 0, bufferSize);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            if (streamWriter != null)
            {
                streamWriter.Close();
                streamWriter = null;
            }
            if (theEntry != null)
            {
                theEntry = null;
            }
            GC.Collect();
            GC.Collect(1);
        }

        public static Dictionary<string, byte[]> UncompressToStreamDictionary(Stream InputStream)
        {
            List<string> ImageExtensionList = new List<string>()
            {
                "gif","jpg","jpeg","png","bmp"
            };
            int bufferSize = 4096;     //指定的緩衝區大小
            byte[] bufferArray = new byte[bufferSize];
            Dictionary<string, byte[]> results = new Dictionary<string, byte[]>();
            ZipEntry theEntry = null;

            using (ZipInputStream unzip = new ZipInputStream(InputStream))
            {
                while ((theEntry = unzip.GetNextEntry()) != null)
                {
                    if (theEntry.Name != string.Empty)
                    {
                        if (ImageExtensionList.Contains(GetExtension(theEntry.Name.ToLower())))//判斷副檔名
                        {
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                while (true)
                                {
                                    bufferSize = unzip.Read(bufferArray, 0, bufferArray.Length);
                                    if (bufferSize > 0)
                                    {
                                        memStream.Write(bufferArray, 0, bufferSize);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (memStream.Length > 0)
                                {
                                    results.Add(GetBaseName(theEntry.Name), memStream.GetBuffer());
                                }
                            }
                        }
                    }
                }
            }

            if (theEntry != null) theEntry = null;
            GC.Collect();
            GC.Collect(1);
            return results;
        }
        // 取得檔名(去除路徑)
        private static string GetBaseName(string fullName)
        {
            string result;
            int lastBackSlash = fullName.LastIndexOf(@"/");
            result = fullName.Substring(lastBackSlash + 1);
            return result;
        }
        // 取得副檔名
        private static string GetExtension(string fullName)
        {
            string result = "";
            int lastBackSlash = fullName.LastIndexOf(@".");
            result = fullName.Substring(lastBackSlash + 1);
            return result;
        }
        /// <summary>
        ///  取得檔名(去除路徑, 副檔名)
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static string GetTureFileName(string fullName)
        {
            string result = "";
            result = GetBaseName(fullName);
            int lastBackSlash = result.LastIndexOf(@".");
            result = result.Substring(0,lastBackSlash);
            return result;
        }
    }
}