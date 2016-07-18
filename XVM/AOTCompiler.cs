using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.CSharp;

namespace XVM
{

    class AOTCompiler
    {
        //Version 0 assembly format
        string TranslateClassName(string inputName)
        {
            return "XLang_"+inputName.Replace(".","_");
        }
        void VersionZero(BinaryReader mreader, StringBuilder outcode)
        {
            int types = mreader.ReadInt32();
            for(int i = 0;i<types;i++)
            {
                string libID = mreader.ReadString(); //Ignored for now.
                string name = mreader.ReadString();
                bool isStruct = mreader.ReadBoolean();
                if (isStruct)
                {
                    int size = mreader.ReadInt32();
                    int alignment = mreader.ReadInt32();
                    if(size != 0)
                    {
                        outcode.AppendLine("[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential,Pack = "+alignment+",Size = "+size+")]");
                    }
                    outcode.AppendLine("public struct " + TranslateClassName(name) + " {");
                    outcode.AppendLine("}");
                }else
                {
                    throw new Exception("Not Yet Implemented");
                }
            }
            outcode.AppendLine("public class XLang_Methods {");
            int methods = mreader.ReadInt32();

            for(int i = 0;i<methods;i++)
            {
                string methodName = mreader.ReadString();
                string returnType = mreader.ReadString();
                if(methodName == "")
                {
                    outcode.AppendLine("public static void Main(");
                }else
                {
                    outcode.Append("public static " + TranslateClassName(returnType) + " " +methodName+"(");
                }
                int argcount = mreader.ReadInt32();
                for(int c = 0;c<argcount;c++) //How C++ was invented
                {
                    string libid = mreader.ReadString();
                    string type = mreader.ReadString();
                    outcode.Append(TranslateClassName(type)+" "+c+(c<argcount-1 ? ",":""));
                }
                outcode.AppendLine(") {");
                outcode.AppendLine("}");
            }
            outcode.AppendLine("}");
        }

        public AOTCompiler(Stream binary)
        {
            
            StringBuilder csharp = new StringBuilder();
            BinaryReader mreader = new BinaryReader(binary);
            switch(mreader.ReadInt32())
            {
                case 0:
                    VersionZero(mreader,csharp);
                    break;
                default:
                    throw new Exception("Unsupported bytecode format.");
            }

            
            CSharpCodeProvider compiler = new CSharpCodeProvider();
                var results = compiler.CompileAssemblyFromSource(new System.CodeDom.Compiler.CompilerParameters(new string[] { }, "xasm_native.dll"),csharp.ToString());
            foreach (var iable in results.Errors)
            {
                Console.WriteLine(iable);
            }
            Console.WriteLine(csharp);

            var assembly = results.CompiledAssembly;

                
        }
    }
}


