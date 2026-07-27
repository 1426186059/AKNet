using KNet.Extentions.Protobuf.Editor;
using System.Reflection;
using TestCommon;

namespace KNetEditorTest
{
    internal class Program
    {
        private static string ProtocPath = GetProtocExePath();
        private static string ProtoPath = GetProtoPath();
        private static string ProtoOutPath = GetProtoOutPath();

        private static string ProtoNameSpaceRootName = "NetProtocols";
        private static string ProtoDLLFilePath = GetProtoDLLFilePath();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            KNetProtoBufEditor.DoPublicCSFile(ProtocPath, ProtoOutPath, ProtoPath);
            KNetProtoBufEditor.DoProtoResetCSFile(ProtoOutPath, ProtoNameSpaceRootName, ProtoDLLFilePath);
        }


        static string GetProtocExePath()
        {
            return Path.Combine(FileTool.GetParentSpecialDir("protoc-28.2-win64"), "bin", "protoc.exe");
        }

        static string GetProtoPath()
        {
            return Path.Combine(FileTool.GetParentSpecialDir("Protobuf"));
        }

        static string GetProtoOutPath()
        {
            return Path.Combine(GetProtoPath(), "Out");
        }

        static string GetProtoDLLFilePath()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Name + ".dll";
        }
    }
}

