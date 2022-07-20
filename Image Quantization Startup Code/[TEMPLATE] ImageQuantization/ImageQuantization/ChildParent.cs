using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Priority_Queue;
namespace ImageQuantization
{
   public class ChildParent: FastPriorityQueueNode
    {
        //use as data type in queue data type 
        public int parent, child;
       public ChildParent(int parent, int child)
       {
            this.parent = parent;  
            this.child = child;
       }
       public ChildParent()
       {

       }
   }
}
