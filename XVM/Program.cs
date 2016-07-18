using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace XVM
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential,Pack =4,Size =4)]
    struct someint
    {

    }

    class Program
    {
        //XLANG virtual machine and runtime
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

        static void Main(string[] args)
        {
            someint myint;
            unsafe
            {
                int* eger = (int*)&myint;
                *eger = 9001;

                someint myOtherInt = myint;
                eger = (int*)&myOtherInt;
            }
            Stream file = File.Open(args[0],FileMode.Open);
            
            AOTCompiler compiler = new AOTCompiler(file);

        }
    }
}
