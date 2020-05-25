using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace BusSchedule1
{
    public class ScheduleState
    {
        internal byte[,,] Schedule { get; }

        internal byte[,,] AvailableShifts ;

        public int CurrentEnergy { get; private set; }

        Dictionary<byte, byte[]> DriverAbilities = new Dictionary<byte, byte[]>();
        Dictionary<byte, byte[]> DriverDaysOff = new Dictionary<byte, byte[]>();

        Dictionary<byte, byte[]> DriverPreferableDaysOff = new Dictionary<byte, byte[]>();

        /*
         * Dic [day, {early = 0 late = 1} ]
         */
        Dictionary<byte, Dictionary<byte, byte>> DriverPreferbaleShift = new Dictionary<byte, Dictionary<byte, byte>>();

        public ScheduleState()
        {
            Schedule = InitSchedule();
            AvailableShifts = InitShifts();
            FillInData();
        }

        public bool IsShiftAlreadyScheduled(int day, int lineNum, int time)
        {
            return AvailableShifts[day, lineNum, time] == 0;
        }

        public bool IsDriverBusyToday(int day, int driver)
        {
            return Schedule[day, driver, 0] != 0 || Schedule[day, driver, 1] != 0;
        }


        public void SetLineToDriver(int lineNum, int driver, int day, int time)
        {
            Schedule[day, driver, time] = (byte) (lineNum + 1);

            //Console.Write("Tile: "+day+" "+driver+" "+time+" set to:"+ Schedule[day, driver, time]);
            //Console.WriteLine("  Is can drive: "+IsDriverCanDriveLine((byte)(driver+1), (byte)(lineNum+1)));

            AvailableShifts[day, lineNum, time] = 0;
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

        public bool IsTodayDayOff(byte driver, byte day)
        {
            byte[] daysOff;
            DriverDaysOff.TryGetValue(driver, out daysOff);

            if (daysOff != null && ((IList) daysOff).Contains(day))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsDriverCanDriveLine(byte driver, byte line)
        {
            byte[] lines = null;
            DriverAbilities.TryGetValue(driver, out lines);
            return lines != null && ((IList) lines).Contains(line);

        }

        public int CalculateEnergy()
        {
            int result = 0;

            // Not scheduled shift
            for (byte i = 0; i < 14; i++)
            {
                for (byte k = 0; k < 2; k++)
                {
                    bool firsLineScheduled = false, secondLineScheduled = false, thirsLineScheduled = false;
                    for (byte j = 0; j < 11; j++)
                    {
                        if (Schedule[i, j, k] == 1)
                        {
                            firsLineScheduled = true;
                        }

                        if (Schedule[i, j, k] == 2)
                        {
                            secondLineScheduled = true;
                        }

                        if (Schedule[i, j, k] == 3)
                        {
                            thirsLineScheduled = true;
                        }
                    }

                    if (firsLineScheduled == false)
                    {
                        result -= 20;
                    }

                    if (secondLineScheduled == false)
                    {
                        result -= 20;
                    }

                    if (thirsLineScheduled == false)
                    {
                        result -= 20;
                    }
                }
            }

            //Console.WriteLine("No scheduled shifts: " + result);

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
                        if (shiftNumber == k && Schedule[i - 1, j - 1, k] != 0)
                        {
                            result += 3;
                        }
                    }
                }
            }

            //Console.WriteLine("Shift preferences" + result);

            // Day-off preference


            for (byte j = 1; j <= 11; j++)
            {
                byte[] prefDaysOff = DriverPreferableDaysOff.GetValueOrDefault(j);
                for (byte i = 1; i <= 14; i++)
                {

                    bool isTodayPrefDayoff = prefDaysOff != null && ((IList) prefDaysOff).Contains(i);

                    if (isTodayPrefDayoff && Schedule[i - 1, j - 1, 0] == 0 && Schedule[i - 1, j - 1, 0] == 0)
                    {
                        result += 4;
                    }

                }
            }

            //Console.WriteLine("Day off pref: " + result);

            // Long rests
            for (byte j = 0; j < 11; j++)
            {
                List<int> gapsLength = new List<int>();
                int currentGapLength = 0;
                for (byte i = 0; i < 14; i++)
                {
                    for (byte k = 0; k < 2; k++)
                    {
                        if (Schedule[i, j, k] == 0)
                        {
                            currentGapLength++;
                        }
                        else if (currentGapLength > 0)
                        {
                            gapsLength.Add(currentGapLength);
                            currentGapLength = 0;
                        }

                        ;
                    }
                }

                if (currentGapLength > 0)
                {
                    gapsLength.Add(currentGapLength);
                }

                ;

                foreach (var gapLength in gapsLength)
                {
                    if (gapLength >= 3)
                    {
                        result += 5;
                    }
                }

            }

            //Console.WriteLine("Long rest:" + result);

            // Early shift after late shift

            for (byte j = 0; j < 11; j++)
            {
                for (byte i = 0; i < 14; i++)
                {
                    if (i < 13 && Schedule[i, j, 1] != 0 && Schedule[i + 1, j, 0] != 0)
                    {
                        result -= 30;
                    }

                }
            }

            //Console.WriteLine("Early shift after late" + result);

            // Not scheduled night shift

            for (byte j = 0; j < 11; j++)
            {
                byte lateShiftsSumStandard = 4;
                for (byte i = 0; i < 14; i++)
                {
                    if (Schedule[i, j, 1] != 0)
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

            //Console.WriteLine("LAte shifts not equal 4: " + result);

            // More than 3 consequently late shifts
            for (byte j = 0; j < 11; j++)
            {
                byte lateSum = 0;

                for (byte i = 0; i < 14; i++)
                {

                    while (Schedule[i, j, 1] != 0)
                    {
                        lateSum++;
                        if (i < 13)
                        {
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lateSum > 3)
                    {
                        result -= (lateSum - 3) * 10;
                        lateSum = 0;
                    }
                }
            }

            //Console.WriteLine("More than 3 consequently late shifts: " + result);
            CurrentEnergy = result;

            return result;
        }

        public bool HasNotScheduledShifts {
            get
            {
                for (int i = 0; i < 14; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            if (this.AvailableShifts[i, j, k] == 1)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
    }

    void FillInData()
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
            DriverPreferbaleShift.Add(3, new Dictionary<byte, byte>() { { 2, 1 }, { 9, 1 }, { 11, 0 } });
            DriverPreferbaleShift.Add(4, new Dictionary<byte, byte>() { { 4, 0 }, { 5, 1 } });
            DriverPreferbaleShift.Add(5, new Dictionary<byte, byte>() { { 8, 1 }, { 11, 1 } });
            DriverPreferbaleShift.Add(6, new Dictionary<byte, byte>() { { 6, 1 }, { 8, 1 }, { 13, 0 } });
            DriverPreferbaleShift.Add(7, new Dictionary<byte, byte>() { { 13, 0 } });
            DriverPreferbaleShift.Add(8, new Dictionary<byte, byte>() { { 4, 0 }, { 8, 0 }, { 14, 1 } });
            DriverPreferbaleShift.Add(9, new Dictionary<byte, byte>() { { 1, 1 } });
            DriverPreferbaleShift.Add(10, new Dictionary<byte, byte>() { { 4, 1 }, { 5, 1 }, { 11, 1 } });
            DriverPreferbaleShift.Add(11, new Dictionary<byte, byte>() { { 3, 0 }, { 7, 0 }, { 14, 1 } });

        }

        public bool IsPrefToWork(byte driver, byte day, byte time)
        {
            Dictionary<byte, byte> currentDriver = DriverPreferbaleShift.GetValueOrDefault(driver);
            byte prefTime = currentDriver.GetValueOrDefault(day,(byte)9);

            if (prefTime == time)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsLeftLassFiveShift()
        {
            int sum = 0;
            for (int i = 0; i < 14; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        sum += AvailableShifts[i, j, k];
                    }
                }
                
            }

            return sum <= 5;
        }

        public ShiftStructure GetFirstAvaliable()
        {
            for (int i = 0; i < 14; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        if (AvailableShifts[i, j, k] == 1)
                        {
                            return new ShiftStructure(){Day = i, Line = j, Time = k, IsLast = true};
                        };
                    }
                }
            }

            return null;
        }


    }
}
