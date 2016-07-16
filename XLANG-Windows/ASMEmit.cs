using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace XLANG_Windows
{
    public class ASMEmitter
    {
        public ASMEmitter(Parser parser, Stream output)
        {
            BinaryWriter mwriter = new BinaryWriter(output);
            mwriter.Write(0); //Version 0
            //Emit all types
            Scope scope = parser.MainMethod.Scope;
            foreach(var iable in scope.types)
            {
                
            }
        }
    }
}
