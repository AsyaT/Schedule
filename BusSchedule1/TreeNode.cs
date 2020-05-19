using System;
using System.Collections.Generic;
using System.Text;

namespace BusSchedule1
{
    public class TreeNode
    {
       

        private TreeNode Parent;
        private List<TreeNode> Children;


        public TreeNode()
        {
            Parent = null;
            Children = new List<TreeNode>();

            ScheduleState = InitSchedule();
            Shifts = InitShifts();
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

        public int Energy => CalculateEnergy(ScheduleState, Shifts);


    }
}
