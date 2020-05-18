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
         * 1 = not scheduled shift
         * 0 =  scheduled shift
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

            for (byte i = 0; i < 14; i++)
            {
                for (byte j = 0; j < 11; j++)
                {
                    for (byte k = 0; k < 2; k++)
                    {
                        Dictionary<byte,byte> currentDriver = DriverPreferbaleShift.GetValueOrDefault(j);

                        if (currentDriver!=null && currentDriver.GetValueOrDefault(i) == k && scheduleState[i, j, k] != 0)
                        {
                            result += 3;
                        }
                    }
                }
            }

            // Day-off preference

            for (byte i = 0; i < 14; i++)
            {
                for (byte j = 0; j < 11; j++)
                {
                    for (byte k = 0; k < 2; k++)
                    {
                        byte[] prefDaysOff = DriverPreferableDaysOff.GetValueOrDefault(j);

                        if (prefDaysOff!=null && prefDaysOff.Contains(i) && scheduleState[i, j, k] == 0)
                        {
                            result += 4;
                        }
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
            //TODO: calculate exactly not equal 4 night shifts
            //TODO: For every late shift assigned that is not equal to 4

            for (byte i = 0; i < 14; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    if (shifts[i, j, 1] == 1)
                    {
                        result -= 8;
                    }
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
            DriverAbilities.Add(1, new byte[] { 2 });
            DriverAbilities.Add(2, new byte[] { 1 });
            DriverAbilities.Add(3, new byte[] { 1 });
            DriverAbilities.Add(4, new byte[] { 2, 3 });
            DriverAbilities.Add(5, new byte[] { 1, 2 });
            DriverAbilities.Add(6, new byte[] { 2, 3 });
            DriverAbilities.Add(7, new byte[] { 2 });
            DriverAbilities.Add(8, new byte[] { 1, 3 });
            DriverAbilities.Add(9, new byte[] { 2 });
            DriverAbilities.Add(10, new byte[] { 3 });
            DriverAbilities.Add(11, new byte[] { 1 });

            DriverDaysOff.Add(1, new byte[] { 3, 4, 10, 11 });
            DriverDaysOff.Add(2, new byte[] { 4, 5, 11, 12 });
            // TODO: to finish

            DriverPreferableDaysOff.Add(1, new byte[] { 8, 14 });
            DriverPreferableDaysOff.Add(2, new byte[] { 7, 8 });
            DriverPreferableDaysOff.Add(3, new byte[] { 1, 10 });
            //TODO: to finish


            DriverPreferbaleShift.Add(1, new Dictionary<byte, byte>() { { 7, 1 }, { 9, 0 }, { 12, 0 }, { 13, 0 } });
            DriverPreferbaleShift.Add(2, new Dictionary<byte, byte>() { { 1, 1 }, { 6, 0 }, { 9, 1 } });
            //TODO : to finish
        }
    }
}
