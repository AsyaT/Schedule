using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BusSchedule1
{
    class Program
    {

       
        static Dictionary<byte, byte[]> DriverAbilities = new Dictionary<byte, byte[]>();
        static Dictionary<byte, byte[]> DriverDaysOff = new Dictionary<byte, byte[]>();
        static Dictionary<byte, byte[]> DriverPreferableDaysOff = new Dictionary<byte, byte[]>();
        /*
         * Dic [day, {early = 0 late = 1} ]
         */
        static Dictionary<byte, Dictionary<byte, byte>> DriverPreferbaleShift = new Dictionary<byte, Dictionary<byte, byte>>(); 

        static void Main(string[] args)
        {
            
            FillInData();

            //Energy = CalculateEnergy(ScheduleState, Shifts);

            TreeNode Root = new TreeNode();

            TreeNode tmpNode = Root;

            for (byte i = 0; i < 14; i++)
            {
                for (byte j = 0; j < 11; j++)
                {
                    for (byte k = 0; k < 2; k++)
                    {
                        
                        
                    }
                }
            }
        }

        void Traverse(TreeNode currentNode, byte i, byte j, byte k)
        {
            List<byte> possibleValues = CalculateAvaliableValues(currentNode, i, j, k /*create param*/);
            foreach (var value in possibleValues)
            {
                var newNode = new TreeNode(i, j, k, value);
                currentNode.Add(newNode);

                byte newI, newJ, newK;



                Traverse(newNode, newI, newJ, newK);
            }
        }

        private static List<byte> CalculateAvaliableValues()
        {

        }

        static void FillInData()
        {
            DriverAbilities.Add(1, new byte[] { 3 });
            DriverAbilities.Add(2, new byte[] { 3 });
            DriverAbilities.Add(3, new byte[] { 1 });
            DriverAbilities.Add(4, new byte[] { 1 });
            DriverAbilities.Add(5, new byte[] { 1 });
            DriverAbilities.Add(6, new byte[] { 1, 3 });
            DriverAbilities.Add(7, new byte[] { 3 });
            DriverAbilities.Add(8, new byte[] { 2 });
            DriverAbilities.Add(9, new byte[] { 2, 3 });
            DriverAbilities.Add(10, new byte[] { 1, 2 });
            DriverAbilities.Add(11, new byte[] { 1, 2 });

            DriverDaysOff.Add(1, new byte[] { 5, 6, 12, 13 });
            DriverDaysOff.Add(2, new byte[] { 6, 7, 13, 14 });
            DriverDaysOff.Add(3, new byte[] { 1, 7, 8, 14 });
            DriverDaysOff.Add(4, new byte[] { 1, 2, 8, 9, });
            DriverDaysOff.Add(5, new byte[] { 2, 3, 9, 10 });
            DriverDaysOff.Add(6, new byte[] { 3, 4, 10, 11 });
            DriverDaysOff.Add(7, new byte[] { 4, 5, 11, 12 });
            DriverDaysOff.Add(8, new byte[] { 5, 6, 12, 13 });
            DriverDaysOff.Add(9, new byte[] { 6, 7, 13, 14 });
            DriverDaysOff.Add(10, new byte[] { 1, 7, 8, 14 });
            DriverDaysOff.Add(11, new byte[] { 1, 2, 8, 9 });

            DriverPreferableDaysOff.Add(1, new byte[] { 3, 8 });
            DriverPreferableDaysOff.Add(2, new byte[] { 1, 9, 10 });
            DriverPreferableDaysOff.Add(3, new byte[] { 3, 5, 10 });
            DriverPreferableDaysOff.Add(4, new byte[] { 14 });
            DriverPreferableDaysOff.Add(5, new byte[] { 6, 12 });
            DriverPreferableDaysOff.Add(6, new byte[] { 7, 14 });
            DriverPreferableDaysOff.Add(7, new byte[] { 1, 7, 8, 9 });
            DriverPreferableDaysOff.Add(8, new byte[] { 1, 9, 10 });
            DriverPreferableDaysOff.Add(9, new byte[] { 2, 3, 4 });
            DriverPreferableDaysOff.Add(10, new byte[] { 3 });
            DriverPreferableDaysOff.Add(11, new byte[] { 5, 6, 13 });

            DriverPreferbaleShift.Add(1, new Dictionary<byte, byte>() { { 1, 1 }, { 10, 0 } });
            DriverPreferbaleShift.Add(2, new Dictionary<byte, byte>() { { 4, 1 }, { 11, 0 } });
            DriverPreferbaleShift.Add(3, new Dictionary<byte, byte>() { { 2,1 }, {9,1}, {11,0} });
            DriverPreferbaleShift.Add(4, new Dictionary<byte, byte>() { { 4,0 },{5, 1} });
            DriverPreferbaleShift.Add(5, new Dictionary<byte, byte>() { { 8,1 }, {11,1} });
            DriverPreferbaleShift.Add(6, new Dictionary<byte, byte>() { { 6,1 },{8,1},{13,0} });
            DriverPreferbaleShift.Add(7, new Dictionary<byte, byte>() { {13,0  } });
            DriverPreferbaleShift.Add(8, new Dictionary<byte, byte>() { { 4,0 }, {8,0}, {14,1} });
            DriverPreferbaleShift.Add(9, new Dictionary<byte, byte>() { {1,1  } });
            DriverPreferbaleShift.Add(10, new Dictionary<byte, byte>() { {4,1  },{5,1}, {11,1} });
            DriverPreferbaleShift.Add(11, new Dictionary<byte, byte>() { {3,0  },{7,0},{ 14,1} });
 
        }
    }
}
