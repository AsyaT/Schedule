using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BusSchedule1
{
    class Program
    {
        private static int Energy = 0;

        /*
         *  Schedule rules
         * 0 = no shift
         * 1 = first line shift
         * 2 = second line shift
         * 3 = third line shift
         */

        static byte[,,] ScheduleState = new byte[14,11,2] ;

        /*
         * 1 = not scheduled shift = shift exists here
         * 0 =  scheduled shift = shift moved to schedule
         */
        static byte[,,] Shifts = new byte[14,3,2];

        static Dictionary<byte, byte[]> DriverAbilities = new Dictionary<byte, byte[]>();
        static Dictionary<byte, byte[]> DriverDaysOff = new Dictionary<byte, byte[]>();
        static Dictionary<byte, byte[]> DriverPreferableDaysOff = new Dictionary<byte, byte[]>();

        /*
         * Dic [day, {early = 0 late = 1} ]
         */
        static Dictionary<byte, Dictionary<byte, byte>> DriverPreferbaleShift = new Dictionary<byte, Dictionary<byte, byte>>(); 

        static void Main(string[] args)
        {
            ScheduleState = InitSchedule();
            Shifts = InitShifts();
            FillInData();

            Energy = CalculateEnergy(ScheduleState, Shifts);

            TreeNode Root = new TreeNode();
            Root.Add(new TreeNode(0, 0, 0, 0));
            Root.Add(new TreeNode(0, 0, 0, 3));
        }

        static int CalculateEnergy(byte[,,] scheduleState, byte[,,] shifts)
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
                        if (shiftNumber == k && scheduleState[i-1, j-1, k] != 0)
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

                    if (isTodayPrefDayoff && scheduleState[i-1, j-1, 0 ] == 0 && scheduleState[i - 1, j - 1, 0] == 0)
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
                        else if(currentGapLength > 0)
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

        static byte[,,] InitSchedule()
        {
            byte[,,] schedule = new byte[14,11,2];
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

        static byte[,,] InitShifts()
        {
            byte[,,] result = new byte[14,3,2];
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
