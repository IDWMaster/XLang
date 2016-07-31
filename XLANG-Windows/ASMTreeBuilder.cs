using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
        public List<ASMType> LocalVariables = new List<ASMType>();
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
                mtype.LibraryID = "";
                
                ResolvedTypes[type] = mtype;
                //TODO: Add functions here
                foreach (var func in type.Functions)
                {
                    Function(func.Value);
                }
            }
            return ResolvedTypes[type];
        } 
        
        void Expression(Expression _exp, BinaryWriter mwriter)
        {
            if (_exp is BinaryExpression)
            {
                //Binary expression (convert to function)
                FunctionCallExpression exp = (_exp as BinaryExpression).ToFunctionCall();
                Expression(exp, mwriter);
            }
            else
            {
                if (_exp is FunctionCallExpression)
                {
                    FunctionCallExpression exp = _exp as FunctionCallExpression;
                    //Push all arguments to stack in reverse order.
                    for (int i = exp.arguments.Count - 1; i >= 0; i--)
                    {
                        Expression(exp.arguments[i], mwriter);
                    }
                    //Invoke function
                    //Call function
                    mwriter.Write((byte)0);
                    mwriter.Write(""); //Current version of compiler doesn't support libraries.... Yet.
                    mwriter.Write(exp.function.GetQualifiedName());
                    
                }else
                {
                    if (_exp is ConstantExpression)
                    {
                        ConstantExpression exp = _exp as ConstantExpression;
                        if(exp.val is int)
                        {
                            mwriter.Write((byte)3);
                            mwriter.Write(".int");
                            mwriter.Write(4);
                            mwriter.Write((int)exp.val);
                        }else
                        {
                            throw new Exception("Not Yet Implemented.");
                        }
                    }
                    else
                    {
                        if (_exp is VariableReferenceExpression)
                        {
                            VariableReferenceExpression exp = _exp as VariableReferenceExpression;
                            if(exp.variable is LocalVariable)
                            {
                                
                            }
                        }else
                        {

                            throw new Exception("Not Yet Implemented.");
                        }
                    }
                }
            }

        }
        ASMFunction Function(XFunction pfunc)
        {
            ASMFunction func = new ASMFunction();
            func.Name = pfunc.GetQualifiedName();
            func.ReturnType = Resolve(pfunc.ReturnType.Resolve());
            
            foreach (var iable in pfunc.args)
            {
                func.Arguments.Add(Resolve(iable.Value.Type.Resolve()));
            }
            foreach(var iable in pfunc.localVars)
            {
                func.LocalVariables.Add(Resolve(iable.Type.Resolve()));
            }
            MemoryStream mstream = new MemoryStream();
            BinaryWriter mwriter = new BinaryWriter(mstream);
            foreach(var iable in pfunc.Operations)
            {
                Expression(iable, mwriter);
            }

            func.Bytecode = mstream.ToArray();
            Functions.Add(func);
            foreach(var iable in pfunc.Scope.types)
            {
                Types.Add(Resolve(iable.Value.Resolve()));
            }
            return func;
        }
        public ASMTreeBuilder(Parser tree)
        {
            //Traverse each scope
            Function(tree.MainMethod);
            
        }
    }
}
