using System;
using System.Collections.Generic;
using System.Text;

namespace BusSchedule1
{
    public class TreeNode
    {
        private byte[,,] ScheduleState = new byte[14, 11, 2];

        /*
         * 1 = not scheduled shift = shift exists here
         * 0 =  scheduled shift = shift moved to schedule
         */
        private byte[,,] Shifts = new byte[14, 3, 2];

        private TreeNode Parent;
        private List<TreeNode> Children;


        public TreeNode()
        {
            Parent = null;
            Children = new List<TreeNode>();
        }

        public TreeNode(byte i, byte j, byte k, byte value)
        {
            Children = new List<TreeNode>();


        }

        public void Add(TreeNode node)
        {
            Children.Add(node);
            node.Parent = this;
        }
    }
}
