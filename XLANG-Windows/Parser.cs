using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLANG_Windows
{
    class StringPointer
    {
        int position = -1;
        string txt;
        public object ReadNumber()
        {

            StringBuilder mbuilder = new StringBuilder();
            while (Next())
            {
                if (!char.IsDigit(Current))
                {
                    Prev();
                    return int.Parse(mbuilder.ToString());
                }

                mbuilder.Append(Current);
            }
            Prev();
            return int.Parse(mbuilder.ToString());

        }
        public StringPointer(string txt)
        {
            this.txt = txt;
        }
        public char Current
        {
            get
            {
                return txt[position];
            }
        }
        public void ReadWhitespace()
        {
            while (Next())
            {
                if (!char.IsWhiteSpace(Current))
                {
                    break;
                }
            }
            Prev();
        }
        public string ExpectIdentifier()
        {
            StringBuilder bodyBuilder = new StringBuilder(); //The one thing you'll never be able to do.
            while (Next())
            {
                if (!char.IsLetterOrDigit(Current))
                {
                    Prev();
                    return bodyBuilder.ToString();
                }
                bodyBuilder.Append(Current);
            }
            Prev();
            return bodyBuilder.ToString();
        }
      

        public string ExpectFunctionName()
        {
            StringBuilder m = new StringBuilder();
            while (Next())
            {
                if (char.IsWhiteSpace(Current))
                {
                    Prev();
                    return m.ToString();
                }
                if (Current == '(' && m.ToString() != "")
                {
                    Prev();
                    return m.ToString();
                }
                m.Append(Current);

            }
            Prev();
            return m.ToString();
        }



        public string Expect(params char[] mander)
        {
            StringBuilder m = new StringBuilder();
            while (Next())
            {
                for (int i = 0; i < mander.Length; i++)
                {
                    if (mander[i] == Current)
                    {
                        Prev();
                        return m.ToString();
                    }
                }
                m.Append(Current);
            }
            Prev();
            return m.ToString();
        }
        public bool Next()
        {
            position++;
            return position < txt.Length;
        }
        public void Prev()
        {
            position--;
        }
    }
    public class XObject
    {
        public XType objType; //Type of object
    }
    public abstract class XType : XObject
    {
    }
    public class ResolvedType:XType
    {

        public int size; //Size of struct (or zero)
        public int alignment; //Alignment of struct
        public bool isStruct;

        static Dictionary<string, XType> types = new Dictionary<string, XType>();
        public Dictionary<string, XType> Fields = new Dictionary<string, XType>();
        public Dictionary<string, XFunction> Functions = new Dictionary<string, XFunction>();
        public ResolvedType(string name)
        {
            if (name != null)
            {
                Name = name;
                types[name] = this;
            }
        }
        public string Name;
    }
    public class UnresolvedType:XType
    {
        public ResolvedType Resolve()
        {
            Scope scope = this.scope;
            while(scope != null)
            {
                if(scope.types.ContainsKey(name))
                {
                    return scope.types[name];
                }
                scope = scope.parent;
            }
            return null;
        }
        Scope scope;
        string name;
        public UnresolvedType(string name, Scope scope)
        {
            this.scope = scope;
            this.name = name;
        }
    }

    public abstract class Variable
    {
        public string Name;
        public XType Type;
        public Variable(string name, XType type)
        {

            Name = name;
            Type = type;
        }
    }
    public class LocalVariable : Variable
    {
        public LocalVariable(string name, XType type) : base(name, type)
        {

        }
    }
    public class Scope
    {
        public Dictionary<string, ResolvedType> types = new Dictionary<string, ResolvedType>(); 
        public Dictionary<string, Variable> locals = new Dictionary<string, Variable>();
        public Scope parent;
        public Scope()
        {

        }
        public Scope(Scope parent)
        {
            this.parent = parent;
        }
    }

    public class XFunction : XObject
    {
        public Scope Scope = new Scope();
        public List<Expression> Operations = new List<Expression>();
        public List<Variable> localVars = new List<Variable>();
        public Dictionary<string, Variable> args = new Dictionary<string, Variable>();
    }
    public class Expression : XObject
    {
    }
    public class VariableReferenceExpression : Expression
    {
        public Variable variable;
        public VariableReferenceExpression(Variable variable)
        {
            this.variable = variable;
        }
    }

    public class CompilationException : Exception
    {
        public CompilationException(string msg) : base(msg)
        {

        }
    }
    public class BinaryExpression : Expression
    {
        public char op;
        public Expression left;
        public Expression right;

    }
    public class ConstantExpression : Expression
    {
        object val;
        public ConstantExpression(object val)
        {
            this.val = val;
        }
    }
    public class Parser
    {
        StringPointer ptr;
        public void Error(string msg)
        {
            throw new CompilationException(msg);
        }
        public Expression Expression(Expression prev)
        {
            while (ptr.Next())
            {
                switch (ptr.Current)
                {
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '=':
                        {
                            BinaryExpression exp = new BinaryExpression();
                            exp.left = prev;
                            exp.op = ptr.Current;
                            ptr.ReadWhitespace();
                            exp.right = Expression(exp);
                            ptr.ReadWhitespace();
                            Expression next = Expression(exp);

                            return next == null ? exp : next;
                        }
                    case ';':
                        ptr.Prev();
                        return null;
                    default:
                        if (char.IsDigit(ptr.Current))
                        {
                            ptr.Prev();
                            Expression exp = new ConstantExpression(ptr.ReadNumber());
                            ptr.ReadWhitespace();
                            Expression next = Expression(exp);

                            return next == null ? exp : next;
                        }
                        Error("Unexpected token " + ptr.Current);
                        return null;
                }
            }
            Error("Unexpected End-of-File (EOF).");
            return null;
        }
        XFunction functionArgs(XFunction func)
        {
            while (ptr.Next())
            {
                ptr.Prev();
                string argType = ptr.ExpectIdentifier();
                ptr.ReadWhitespace();
                string argName = ptr.ExpectIdentifier();
                ptr.ReadWhitespace();
                ptr.Next();
                switch (ptr.Current)
                {
                    case ',':
                        {
                            func.args.Add(argName, new LocalVariable(argName, new UnresolvedType(argType,func.Scope)));
                        }
                        break;
                    case ')':
                        func.args.Add(argName, new LocalVariable(argName, new UnresolvedType(argType,func.Scope)));
                        goto Velociraptor;
                    default:
                        Error("Expected ')'.");
                        break;
                }


            }
            Velociraptor:

            return func;
        }
        ResolvedType ClassBody(ResolvedType type)
        {
            while (ptr.Next())
            {
                if(ptr.Current == '}')
                {
                    return type;
                }
                ptr.Prev();
                string typename = ptr.ExpectIdentifier();
                ptr.ReadWhitespace();
                ptr.Next();
                if (!(char.IsDigit(ptr.Current) || char.IsLetter(ptr.Current)))
                {
                    ptr.Prev();
                    //Must be method (operator overload)
                    string funcName = ptr.ExpectFunctionName();

                    ptr.ReadWhitespace();
                    ptr.Next();
                    if (ptr.Current != '(')
                    {
                        Error("Expected '('.");
                    }
                    ptr.ReadWhitespace();
                    XFunction func = new XFunction();
                    functionArgs(func);
                    type.Functions[funcName] = func;
                    ptr.ReadWhitespace();
                    ptr.Next();
                    if (!(ptr.Current == ';' || ptr.Current == '{'))
                    {
                        Error("Expected function.");
                    }
                    if (ptr.Current == '{')
                    {
                        FunctionBody(func);
                    }

                    ptr.ReadWhitespace();

                }
                else
                {
                    Error("Not yet implemented...");
                }
            }
            return type;
        }
        ResolvedType Struct()
        {
            ResolvedType retval = new ResolvedType(ptr.ExpectIdentifier());
            retval.isStruct = true;
            ptr.ReadWhitespace();
            ptr.Next();
            if (char.IsDigit(ptr.Current))
            {
                //Size.Alignment
                ptr.Prev();
                //Read until 
                retval.size = int.Parse(ptr.Expect('.'));
                ptr.Next();
                retval.alignment = (int)ptr.ReadNumber();
                ptr.ReadWhitespace();
                ptr.Next();

            }
            if (ptr.Current != '{')
            {
                Error("Expected {");
            }
            ptr.ReadWhitespace();
            ClassBody(retval);
            return retval;
        }
        public XFunction FunctionBody(XFunction function)
        {
            var scope = function.Scope;
            while (ptr.Next())
            {
                ptr.Prev();
                string id = ptr.ExpectIdentifier();
                ptr.ReadWhitespace();
                if (id == "struct")
                {
                    ResolvedType type = Struct();
                    scope.types[type.Name] = type;
                    ptr.ReadWhitespace();
                }
                else
                {
                    ptr.Next();
                    Expression exp = null;
                    if (char.IsLetter(ptr.Current))
                    {
                        //Variable declaration (local scope)
                        ptr.Prev();
                        string varName = ptr.ExpectIdentifier();
                        var local = new LocalVariable(varName, new UnresolvedType(id,scope));
                        if (scope.locals.ContainsKey(varName))
                        {
                            Error("Invalid redeclaration of " + varName + ".");
                        }
                        function.localVars.Add(local);
                        scope.locals.Add(varName, local);
                        exp = new VariableReferenceExpression(local);
                    }
                    ptr.ReadWhitespace();
                    ptr.Next();
                    if (ptr.Current == ';')
                    {
                        //End of expression
                        ptr.ReadWhitespace();

                    }
                    else
                    {
                        //Parse expression
                        ptr.Prev();
                        exp = Expression(exp);
                        function.Operations.Add(exp);
                        ptr.Next();
                        ptr.ReadWhitespace();
                    }
                }
            }
            return function;
        }
        public XFunction Main()
        {
            ptr.ReadWhitespace();

            return FunctionBody(new XFunction());
        }
        
        public XFunction MainMethod;

        public Parser(string txt)
        {
            CoreLibrary.Initialize();
            //Parse
            ptr = new StringPointer(txt);
            MainMethod = Main();

        }
    }
}
