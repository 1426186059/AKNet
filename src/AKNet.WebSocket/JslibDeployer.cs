/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/825126369/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 04:26:51
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using System;
using System.IO;
using System.Reflection;

namespace AKNet.WebSocket
{
    internal static class JslibDeployer
    {
        private static bool bDeployed = false;

        /// <summary>
        /// 尝试将 AKNet.WebSocket.jslib 部署到 Unity 项目的 Assets/Plugins/WebGL/ 目录。
        /// 通过反射检测 Unity Editor 环境，安全用于 .NET 库项目。
        /// </summary>
        internal static void TryDeploy()
        {
            if (bDeployed) return;

            try
            {
                // 检查是否在 Unity Editor 中运行
                Type editorAppType = Type.GetType("UnityEditor.EditorApplication, UnityEditor");
                if (editorAppType == null) return;

                string dataPath = GetUnityDataPath();
                if (string.IsNullOrEmpty(dataPath)) return;

                string targetDir = Path.Combine(dataPath, "..", "Assets", "Plugins", "WebGL");
                string targetPath = Path.Combine(targetDir, "AKNet.WebSocket.jslib");

                if (File.Exists(targetPath)) { bDeployed = true; return; }

                var assembly = typeof(JslibDeployer).Assembly;
                string[] resources = assembly.GetManifestResourceNames();
                string jslibResource = null;
                foreach (var r in resources)
                {
                    if (r.EndsWith(".jslib", StringComparison.OrdinalIgnoreCase))
                    {
                        jslibResource = r;
                        break;
                    }
                }

                if (jslibResource == null) return;

                Directory.CreateDirectory(targetDir);
                using (var stream = assembly.GetManifestResourceStream(jslibResource))
                using (var fs = File.Create(targetPath))
                {
                    stream.CopyTo(fs);
                }

                bDeployed = true;
                Console.WriteLine("[AKNet.WebSocket] JSLib deployed to " + targetPath);
            }
            catch
            {
                // 静默失败，不影响正常逻辑
            }
        }

        private static string GetUnityDataPath()
        {
            try
            {
                var applicationType = Type.GetType("UnityEngine.Application, UnityEngine");
                if (applicationType == null) return null;
                var prop = applicationType.GetProperty("dataPath",
                    BindingFlags.Public | BindingFlags.Static);
                if (prop == null) return null;
                return prop.GetValue(null) as string;
            }
            catch
            {
                return null;
            }
        }
    }
}
