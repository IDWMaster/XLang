using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace XVM
{
    class Program
    {
        //XLANG virtual machine and runtime
        static void Main(string[] args)
        {
            BinaryReader mreader = new BinaryReader(File.Open(args[0], FileMode.Open));
            //Each XVM binary is composed of a series of XTypes, followed by functions
            //Each XTYPE has fields, which have an XType and a name
            //Each function has a name, and a series of instructions
            //The instruction set is a function-based stack-machine.
            
            //Strings are NULL-terminated, UTF-8 encoded values
            //
            //Instruction encoding:
            //OPCODE 0 -- Call function = (string)libName,(string)FunctionName
            //OPCODE 1 -- stloc.i where I is a 32-bit index of a local variable
            //OPCODE 2 -- ldloc.i where I is a 32-bit index of a local variable
            //OPCODE 3 -- ldconst.T where T is (string)TypeOfData = byte array prefixed with 32-bit length
            
        }
    }
}
