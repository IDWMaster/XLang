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
        public abstract ResolvedType Resolve();
    }
    public class ResolvedType:XType
    {
        public override ResolvedType Resolve()
        {
            return this;
        }

        public int size; //Size of struct (or zero)
        public int alignment; //Alignment of struct
        public bool isStruct;
        public Scope scope;
        public static Dictionary<string, XType> types = new Dictionary<string, XType>();
        public Dictionary<string, XType> Fields = new Dictionary<string, XType>();
        public Dictionary<string, XFunction> Functions = new Dictionary<string, XFunction>();
        public string GetQualifiedName()
        {
            Queue<string> nameQueue = new Queue<string>();
            Scope scope = this.scope;
            while(scope != null)
            {
                if (scope.Name != "")
                {
                    nameQueue.Enqueue(scope.Name);
                }
                scope = scope.parent;
            }
            StringBuilder mbuilder = new StringBuilder();
            while(nameQueue.Any())
            {
                mbuilder.Append(nameQueue.Dequeue());
            }
            return mbuilder.ToString()+"."+Name;
        }
        public ResolvedType(string name, Scope scope)
        {
            this.scope = scope;
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
        public override ResolvedType Resolve()
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
        //Name of scope, or NULL for global scope.
        public string Name;
        public Dictionary<string, ResolvedType> types = new Dictionary<string, ResolvedType>(); 
        public Dictionary<string, Variable> locals = new Dictionary<string, Variable>();
        public Scope parent;
        public List<Scope> childScopes = new List<Scope>();
        public Scope()
        {
            
        }
        public Scope(Scope parent)
        {
            this.parent = parent;
            parent.childScopes.Add(this);
        }
    }

    public class XFunction : XObject
    {
        public Scope Scope = new Scope();
        public List<Expression> Operations = new List<Expression>();
        public List<Variable> localVars = new List<Variable>();
        public Dictionary<string, Variable> args = new Dictionary<string, Variable>();
        public XType ReturnType;
        public string Name;
        public XFunction(string name)
        {
            Name = name;
        }
        public string GetQualifiedName()
        {
            Queue<string> nameQueue = new Queue<string>();
            Scope scope = Scope;
            while (scope != null)
            {
                if (scope.Name != "")
                {
                    nameQueue.Enqueue(scope.Name);
                }
                scope = scope.parent;
            }
            StringBuilder mbuilder = new StringBuilder();
            while (nameQueue.Any())
            {
                mbuilder.Append(nameQueue.Dequeue());
            }
            return mbuilder.ToString() + "." + Name;
        }
    }
    public class FunctionCallExpression:Expression
    {
        public XFunction function;
        public List<Expression> arguments = new List<Expression>();
        public override XType ResolveType()
        {
            return function.ReturnType;
        }
    }
    public abstract class Expression
    {
        public abstract XType ResolveType();
    }
    public class VariableReferenceExpression : Expression
    {
        public Variable variable;
        public VariableReferenceExpression(Variable variable)
        {
            this.variable = variable;
        }
        public override XType ResolveType()
        {
            return variable.Type;
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
        public FunctionCallExpression ToFunctionCall()
        {
            FunctionCallExpression espn = new FunctionCallExpression(); //Extra-sensory perception network
            //TODO: Resolve function here
            ResolvedType concrete = left.ResolveType().Resolve();
            
            espn.function = concrete.Functions[op.ToString()];
            espn.arguments.Add(left);
            espn.arguments.Add(right);
            return espn;
        }
        public override XType ResolveType()
        {
            return ToFunctionCall().ResolveType();
        }


    }
    public class ConstantExpression : Expression
    {
        public object val;
        public ConstantExpression(object val)
        {
            this.val = val;
        }
        public override XType ResolveType()
        {
            throw new NotImplementedException();
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
                    XFunction func = new XFunction(funcName);
                    func.Scope.parent = type.scope;
                    func.ReturnType = new UnresolvedType(typename, func.Scope);
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
        ResolvedType Struct(Scope scope)
        {
            scope = new Scope(scope);
            ResolvedType retval = new ResolvedType(ptr.ExpectIdentifier(),scope);
            scope.Name = retval.Name;
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
                    ResolvedType type = Struct(scope);
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

            return FunctionBody(new XFunction(""));
        }
        
        public XFunction MainMethod;
        ResolvedType XVoid;

        public Parser(string txt)
        {
            XVoid = new ResolvedType("xvoid",null);
            XVoid.isStruct = true;
            XVoid.size = 0;
            CoreLibrary.Initialize();
            //Parse
            ptr = new StringPointer(txt);
            MainMethod = Main();
            MainMethod.ReturnType = XVoid;

        }
    }
}
