using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XVM
{
    public class FieldInfo
    {
        public TypeInfo Type;
        public string Name;
    }
    public class TypeInfo
    {
        public string Name;
        public Dictionary<string, FieldInfo> Fields = new Dictionary<string, FieldInfo>();
    }
    public class FunctionInfo
    {
        public string Name;
        public List<TypeInfo> args = new List<TypeInfo>();
        
    }
    public class BinaryIntrinsic:FunctionInfo
    {
        public BinaryIntrinsic(TypeInfo left, TypeInfo right)
        {
            args.Add(left);
            args.Add(right);
        }
    }
    
    public class Module
    {
        public static Dictionary<string, Module> loadedModules = new Dictionary<string, Module>();
        public Dictionary<string, TypeInfo> Types = new Dictionary<string, TypeInfo>();
        public Dictionary<string, FunctionInfo> Functions = new Dictionary<string, FunctionInfo>();
        public static FunctionInfo ResolveFunction(string module, string function)
        {
            return loadedModules[module].Functions[function];
        }
        public static TypeInfo ResolveType(string module, string type)
        {
            return loadedModules[module].Types[module];
        }
    }
}
