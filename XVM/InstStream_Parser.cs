using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace XVM
{
    public class FunctionCallExpression:Expression
    {
        public FunctionInfo func;
        public List<Expression> args = new List<Expression>();
        public FunctionCallExpression(FunctionInfo info)
        {
            func = info;
        }
    }
    public class Expression
    {
        public string type;
    }
    public class ConstantExpression:Expression
    {
        byte[] val;
        public ConstantExpression(byte[] val, string type)
        {
            this.val = val;
            this.type = type;
        }
    }
    class InstStream_Parser
    {
        public List<Expression> expressions = new List<Expression>();
        public Stack<Expression> stack = new Stack<Expression>();
        public InstStream_Parser(BinaryReader mreader)
        {
            while (mreader.BaseStream.Position != mreader.BaseStream.Length)
            {
                byte me = mreader.ReadByte();
                switch (me)
                {
                    case 0:
                        {
                            string library = mreader.ReadString(); //Library id
                            string name = mreader.ReadString();
                            FunctionCallExpression exp = new FunctionCallExpression(Module.ResolveFunction(library,name));
                            //TODO: Pop all arguments from the stack. We need to somehow resolve the function.
                            for(int i = 0;i<exp.args.Count;i++)
                            {
                                Expression arg = stack.Pop();
                                
                            }
                            expressions.Add(exp);
                            stack.Push(exp);
                        }
                        break;
                    case 3:
                        {
                            //Load constant on evaluation stack
                            string type = mreader.ReadString();
                            stack.Push(new ConstantExpression(mreader.ReadBytes(mreader.ReadInt32()), type));
                        }
                        break;
                    default:
                        throw new Exception("Not yet implemented.");
                }
            }
        }
    }
}
