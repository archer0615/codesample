using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ROG.Commons.Helpers
{
    public enum ManifestMode
    {
        JavaScript,
        Css,
        Image
    }
    public class ManifestHelper
    {
        private const string srcBasePath = "/support/";
        private const string jsPre = @"<script type='text/javascript' src='";
        private const string jsLast = @"'></script>";
        private const string cssPre = @"<link rel='stylesheet' type='text/css' media='all' href='";
        private const string cssLast = @"'>";
        private string FilePath { get; set; }
        private ManifestMode ImportMode { get; set; }
        private Dictionary<string, string> All_data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">Manifest檔案路徑</param>
        public ManifestHelper(string filePath)
        {
            this.FilePath = filePath;
            All_data = GetManifestFileData();
        }
        /// <summary>
        /// 取得JS_Hash路徑
        /// </summary>
        /// <returns></returns>
        public string GetJavaScriptSrc()
        {
            var JsSrcList = All_data.Where(x => x.Key.Contains(".js")).Select(x => x.Value).ToList();
            string jsSrcString = string.Empty;
            foreach (var item in JsSrcList)
            {
                jsSrcString += $"{jsPre}{srcBasePath + item}{jsLast}\n";
            }
            return jsSrcString;
        }
        /// <summary>
        /// 取得CSS_Hash路徑
        /// </summary>
        /// <returns></returns>
        public string GetCssSrc()
        {
            var CssSrcList = All_data.Where(x => x.Key.Contains(".css")).Select(x => x.Value).ToList();
            string cssSrcString = string.Empty;
            foreach (var item in CssSrcList)
            {
                cssSrcString += $"{cssPre}{srcBasePath + item}{cssLast}\n";
            }
            return cssSrcString;
        }
        private Dictionary<string, string> GetManifestFileData()
        {
            string baseFilePath = AppDomain.CurrentDomain.BaseDirectory;
            JObject o1 = JObject.Parse(File.ReadAllText(baseFilePath + FilePath));

            Dictionary<string, string> results =
                ((IDictionary<string, JToken>)o1).ToDictionary(pair => pair.Key, pair => (string)pair.Value);

            return results;
        }
    }
}