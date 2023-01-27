using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiraliMauvaModelAdapter.IO
{
    public class MA_Node
    {
        public string NodeType;
        public NPart[] Parts;
        public string UID_R;

    }

    public struct NPart
    {
        char[] idt;
        string name;
    }

    public struct NAttribute
    {
        //setAttr    -k  off  ".v"   no;
        //          idt  idv  call   call_val

        //setAttr   ".rnd"    no;
        //          call     call_val

        //setAttr ".t"    -type     "double3" -110.74483232002081 28.350614059855587 2.8805602357781055 ;
        //         call  call_idt   cidt_name  cidt_value
        //setAttr ".imn"  -type     "string"    "persp";

        // cannot break up with spaces! will need to be more careful!
        // CAN break up with spaces if the space is not within ""!
        // this why \/
        // setAttr ".hc" -type "string" "viewSet -p %camera";

        // setAttr ".pt[629:703]"           -1.6997 4.60010004 7.47829962 -0.80989999 2.15409994 
        //         call_arr int_a int_b      call_val

        //setAttr ".fc[0:499]"         -type       "polyFaces" f 4 0 1 2 3
        //       call_arr int_a int_b  call_idt    cidt_name   cidt_value


        char[] idt;
        string idv;
        string call_tag;

    }
}
