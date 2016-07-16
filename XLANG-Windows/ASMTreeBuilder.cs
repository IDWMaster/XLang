using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLANG_Windows
{
    public class ASMVar
    {
        public string Name { get; set; }
        public ASMType Type { get; set; } //ASM type
    }
    public class ASMType
    {
        public string LibraryID; //Library name; in form folder.folder....libname
        public string Name; //Fully-qualified name of type
        public bool IsStruct; //Whether or not this type is a struct
        public int Size; //Size of type (ONLY emitted for structs)
        public int Alignment; //Alignment of type (ONLY emitted for structs)
        
    }
    public class ASMFunction
    {
        public string Name;
        public byte[] Bytecode;
        public List<ASMType> Arguments = new List<ASMType>();
        public ASMType ReturnType;
    }
    class ASMTreeBuilder
    {
        public List<ASMFunction> Functions = new List<ASMFunction>();
        public List<ASMType> Types = new List<ASMType>();
        Dictionary<ResolvedType, ASMType> ResolvedTypes = new Dictionary<ResolvedType, ASMType>();
        ASMType Resolve(ResolvedType type)
        {
            if(!ResolvedTypes.ContainsKey(type))
            {
                ASMType mtype = new ASMType();
                mtype.Alignment = type.alignment;
                mtype.Size = type.size;
                mtype.IsStruct = type.isStruct;
                mtype.Name = type.GetQualifiedName();
                ResolvedTypes[type] = mtype;
            }
            return ResolvedTypes[type];
        } 
        
        void Function(XFunction pfunc)
        {
            ASMFunction func = new ASMFunction();
            func.Name = pfunc.Name;
            func.ReturnType = pfunc.ReturnType;

        }
        public ASMTreeBuilder(Parser tree)
        {
            //Traverse each scope
            Function(tree.MainMethod);
        }
    }
}
