using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XVM
{

    class Builtins
    {
        public static void init()
        {
            var module = new Module();
            Module.loadedModules[""] = module;
            
        }
    }
}
