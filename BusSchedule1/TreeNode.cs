using System;
using System.Collections.Generic;
using System.Text;

namespace BusSchedule1
{
    public class TreeNode
    {
        /*
        *  Schedule rules
        * 0 = no shift
        * 1 = first line shift
        * 2 = second line shift
        * 3 = third line shift
        */
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

        private int CalculateEnergy(byte[,,] scheduleState, byte[,,] shifts)
        {
            int result = 0;

            // Not scheduled shift
            for (byte i = 0; i < 14; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    for (byte k = 0; k < 2; k++)
                    {
                        if (shifts[i, j, k] == 1)
                        {
                            result -= 20;
                        }
                    }
                }
            }

            // Shift preferences
            for (byte j = 1; j <= 11; j++)
            {
                Dictionary<byte, byte> currentDriver = DriverPreferbaleShift.GetValueOrDefault(j);
                for (byte i = 1; i <= 14; i++)
                {
                    byte shiftNumber;
                    currentDriver.TryGetValue(i, out shiftNumber);

                    for (byte k = 0; k < 2; k++)
                    {
                        if (shiftNumber == k && scheduleState[i - 1, j - 1, k] != 0)
                        {
                            result += 3;
                        }
                    }
                }
            }

            // Day-off preference


            for (byte j = 1; j <= 11; j++)
            {
                byte[] prefDaysOff = DriverPreferableDaysOff.GetValueOrDefault(j);
                for (byte i = 1; i <= 14; i++)
                {

                    bool isTodayPrefDayoff = prefDaysOff != null && prefDaysOff.Contains(i);

                    if (isTodayPrefDayoff && scheduleState[i - 1, j - 1, 0] == 0 && scheduleState[i - 1, j - 1, 0] == 0)
                    {
                        result += 4;
                    }

                }
            }

            // Long rests
            for (byte j = 0; j < 11; j++)
            {
                List<int> gapsLength = new List<int>();
                int currentGapLength = 0;
                for (byte i = 0; i < 14; i++)
                {
                    for (byte k = 0; k < 2; k++)
                    {
                        if (scheduleState[i, j, k] == 0)
                        {
                            currentGapLength++;
                        }
                        else if (currentGapLength > 0)
                        {
                            gapsLength.Add(currentGapLength);
                            currentGapLength = 0;
                        };
                    }
                }

                if (currentGapLength > 0)
                {
                    gapsLength.Add(currentGapLength);
                };

                foreach (var gapLength in gapsLength)
                {
                    if (gapLength >= 3)
                    {
                        result += 5;
                    }
                }

            }

            // Early shift after late shift

            for (byte j = 0; j < 11; j++)
            {
                for (byte i = 0; i < 14; i++)
                {
                    if (scheduleState[i, j, 1] != 0 && scheduleState[i + 1, j, 0] != 0)
                    {
                        result -= 30;
                    }

                }
            }

            // Not scheduled night shift
            //TODO: For every late shift assigned that is not equal to 4

            for (byte j = 0; j < 11; j++)
            {
                byte lateShiftsSumStandard = 4;
                for (byte i = 0; i < 14; i++)
                {
                    if (scheduleState[i, j, 1] != 0)
                    {
                        lateShiftsSumStandard--;
                    }
                }

                if (lateShiftsSumStandard > 0)
                {
                    result -= lateShiftsSumStandard * 8;
                }
                else
                {
                    result += lateShiftsSumStandard * 8;
                }

            }

            // More than 3 consequently late shifts
            for (byte j = 0; j < 11; j++)
            {
                byte lateSum = 0;

                for (byte i = 0; i < 14; i++)
                {

                    while (scheduleState[i, j, 1] != 0)
                    {
                        lateSum++;
                        i++;
                    }

                    if (lateSum > 3)
                    {
                        result -= (lateSum - 3) * 10;
                        lateSum = 0;
                    }
                }


            }


            return result;
        }

        private static byte[,,] InitSchedule()
        {
            byte[,,] schedule = new byte[14, 11, 2];
            for (byte i = 0; i < 14; i++)
            {
                for (byte j = 0; j < 11; j++)
                {
                    for (byte k = 0; k < 2; k++)
                    {
                        schedule[i, j, k] = 0;
                    }
                }
            }

            return schedule;
        }

        private static byte[,,] InitShifts()
        {
            byte[,,] result = new byte[14, 3, 2];
            for (byte i = 0; i < 14; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    for (byte k = 0; k < 2; k++)
                    {
                        result[i, j, k] = 1;
                    }
                }
            }

            return result;
        }
    }
}
