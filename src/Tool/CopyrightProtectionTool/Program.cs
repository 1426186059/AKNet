using System.Text;

namespace CopyrightProtectionTool;

internal class Program
{
    const string Head = "/************************************Copyright*****************************************";
    const string End   = "************************************Copyright*****************************************/";

    static readonly string[] DirList =
    {
        "KNet.Common",
        "KNet",
        "KNet.Extentions.Protobuf",
        "KNet.MSQuic",
        "KNet.Quic",
        "KNet.WebSocket",
        "KNet.Platform",
        "KNet.LinuxTcp"
    };

    static readonly string[] SkipDirs = { "bin", "obj", ".git", ".vs" };

    static int s_totalFiles;
    static int s_updatedFiles;
    static int s_errorFiles;
    static bool s_dryRun;

    static int Main(string[] args)
    {
        s_dryRun = args.Contains("--dry-run") || args.Contains("-n");

        string? slnDir = FindSlnDir();
        if (slnDir == null)
        {
            Console.Error.WriteLine("错误: 找不到 .sln 文件，请在仓库根目录下运行此工具。");
            return 1;
        }

        Console.WriteLine($"工作目录: {slnDir}");
        if (s_dryRun) Console.WriteLine("[预览模式] 不会实际修改文件\n");
        Console.WriteLine();

        string copyrightContent = GetCopyrightContent();

        foreach (string dirName in DirList)
        {
            string codeDir = Path.Combine(slnDir, dirName);
            if (!Directory.Exists(codeDir))
            {
                Console.WriteLine($"跳过不存在的目录: {dirName}");
                continue;
            }

            foreach (string filePath in Directory.GetFiles(codeDir, "*.cs", SearchOption.AllDirectories))
            {
                if (ShouldSkip(filePath))
                    continue;

                ProcessFile(filePath, copyrightContent);
            }
        }

        Console.WriteLine();
        Console.WriteLine($"========== 完成 ==========");
        Console.WriteLine($"总文件数 : {s_totalFiles}");
        Console.WriteLine($"已更新   : {s_updatedFiles}");
        if (s_errorFiles > 0)
            Console.WriteLine($"出错     : {s_errorFiles}");
        if (s_dryRun)
            Console.WriteLine("(预览模式，文件未实际修改)");

        return s_errorFiles > 0 ? 1 : 0;
    }

    static string? FindSlnDir()
    {
        // 从当前目录向上查找 .sln 文件，直到根目录
        string dir = Directory.GetCurrentDirectory();
        while (true)
        {
            if (Directory.GetFiles(dir, "*.sln").Length > 0)
                return dir;

            string? parent = Path.GetDirectoryName(dir);
            if (parent == null || parent == dir) break;
            dir = parent;
        }
        return null;
    }

    static bool ShouldSkip(string filePath)
    {
        // 跳过 bin/obj 等生成目录
        string relative = filePath.Replace('\\', '/');
        foreach (string skip in SkipDirs)
        {
            if (relative.Contains($"/{skip}/"))
                return true;
        }
        return false;
    }

    static string GetCopyrightContent()
    {
        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CodeSnippets.template.txt");
        if (!File.Exists(templatePath))
        {
            Console.Error.WriteLine($"错误: 模板文件不存在: {templatePath}");
            Environment.Exit(1);
        }

        string templateContent = File.ReadAllText(templatePath, Encoding.UTF8);

        var replacements = new Dictionary<string, string>
        {
            ["$HEAD$"]        = Head,
            ["$END$"]         = End,
            ["$ProjectName$"] = "KNet",
            ["$Web$"]         = "https://github.com/1426186059/KNet",
            ["$Author$"]      = "许珂",
            ["$StartTime$"]   = "2024/11/01 00:00:00",
            ["$ModifyTime$"]  = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
            ["$Description$"] = "C# 游戏网络库",
            ["$Copyright$"]   = "作者保留一切版权权利, 商业用途需支付版权费用",
            ["$Contact$"]     = "微信：AAA-2025-666-888"
        };

        string result = templateContent;
        foreach (var kv in replacements)
            result = result.Replace(kv.Key, kv.Value);

        return result;
    }

    static void ProcessFile(string filePath, string copyrightContent)
    {
        s_totalFiles++;
        Console.WriteLine(filePath);

        try
        {
            string code = File.ReadAllText(filePath, Encoding.UTF8);
            string original = code;

            // 移除已有的版权头（可能有多层）
            while (code.TrimStart().StartsWith(Head))
            {
                int startIdx = code.IndexOf(Head, StringComparison.Ordinal);
                int endIdx = code.IndexOf(End, startIdx, StringComparison.Ordinal);
                if (endIdx < 0) break;

                int removeLen = endIdx + End.Length - startIdx;
                code = code.Remove(startIdx, removeLen);
            }

            // 去除版权头后的空白行
            code = code.TrimStart();

            // 添加新版权头
            code = copyrightContent + Environment.NewLine + code;

            if (code == original)
            {
                // 没有变化
                return;
            }

            s_updatedFiles++;

            if (!s_dryRun)
            {
                File.WriteAllText(filePath, code, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            s_errorFiles++;
            Console.Error.WriteLine($"  错误: {ex.Message}");
        }
    }
}
