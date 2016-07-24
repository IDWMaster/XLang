using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace XLANG_Windows
{
    class ASMEmitter
    {
        public ASMEmitter(ASMTreeBuilder tree, Stream output)
        {
            BinaryWriter mwriter = new BinaryWriter(output);
            mwriter.Write(0); //XASM version 0
            mwriter.Write(tree.Types.Count);
            foreach (var type in tree.Types)
            {
                mwriter.Write(type.LibraryID);
                mwriter.Write(type.Name);
                mwriter.Write(type.IsStruct);
                if(type.IsStruct)
                {
                    mwriter.Write(type.Size);
                    mwriter.Write(type.Alignment);
                }
            }


            mwriter.Write(tree.Functions.Count);
            foreach (var iable in tree.Functions)
            {
                mwriter.Write(iable.Name); //Function name
                mwriter.Write(iable.ReturnType.Name);
                mwriter.Write(iable.Arguments.Count); //Number of arguments
                foreach (var gument in iable.Arguments)
                {
                    mwriter.Write(gument.LibraryID);
                    mwriter.Write(gument.Name);
                }
                mwriter.Write(iable.LocalVariables.Count);
                //Locals
                foreach (var local in iable.LocalVariables)
                {
                    mwriter.Write(local.LibraryID);
                    mwriter.Write(local.Name);
                }
                //Method body
                mwriter.Write(iable.Bytecode.Length);
                mwriter.Write(iable.Bytecode);
            }
        }
    }
}
