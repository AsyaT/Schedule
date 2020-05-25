using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BusSchedule1
{
    class Program
    {
        /*
       *  Schedule rules
       * 0 = no shift
       * 1 = first line shift
       * 2 = second line shift
       * 3 = third line shift
       */
        static ScheduleState ScheduleState ;

        

        static void Main(string[] args)
        {
            ScheduleState = new ScheduleState();

            ScheduleState result = SimulatedAnnealing(ScheduleState, 10000.0, 1.0);

            PrintSchedule(result);
            PrintLeftShifts(result.AvailableShifts);

        }

        static ScheduleState SimulatedAnnealing(ScheduleState incomeState, double initialTemperature, double endTemperature)
        {
            ScheduleState state = incomeState;
            int currentEnergy = state.CalculateEnergy();
            int candidateEnergy;
            ScheduleState stateCandidate;
            double p;
            /*
             * 1 = not scheduled shift = shift exists here
             * 0 =  scheduled shift = shift moved to schedule
             */
             

            double temperature = initialTemperature;

            for (int iteration = 0; iteration < 10000; iteration++)
            {
                stateCandidate = GenerateStateCandidate(state);
                candidateEnergy = stateCandidate.CalculateEnergy();
                

                if (candidateEnergy > currentEnergy)
                {
                    currentEnergy = candidateEnergy;
                    state = stateCandidate;
                }
                else
                {
                    p = GetTransitionProbability(currentEnergy- candidateEnergy, temperature); 
                    if (IsTransition(p)) 
                    { 
                        currentEnergy = candidateEnergy;
                        state = stateCandidate;
                   
                    }
                }

                Console.WriteLine("Candidate energy: " + currentEnergy);

                temperature = DecreaseTemperature(initialTemperature, iteration); // ??? (temperature, iteration)


                if (temperature <= endTemperature || (state.HasNotScheduledShifts == false))
                {
                    return state;
                }
            }

            return state;
        }

        private static ScheduleState GenerateStateCandidate(ScheduleState state)
        {
            ShiftStructure shift = SearchForShift(state);

            int day = shift.Day;
            int time = shift.Time;
            int lineNum = shift.Line;

            int? driver = SearchForDriver(state, day, lineNum);

            while (driver == null)
            {
                shift = SearchForShift(state);

                day = shift.Day;
                time = shift.Time;
                lineNum = shift.Line;

                driver = SearchForDriver(state, day, lineNum);
            }

            state.SetLineToDriver(lineNum,  driver.Value, day, time);

            return state;
        }

        private static ShiftStructure SearchForShift(ScheduleState state)
        {
            Random random = new Random((int)DateTime.Now.Ticks);

            int dayMax = 14;
            int lineMax = 3;
            int timeMax = 2;

            ShiftStructure shift = new ShiftStructure();

            shift.Day = random.Next(0, dayMax);
            shift.Line = random.Next(0, lineMax);
            shift.Time = random.Next(0, timeMax);

            while (state.IsShiftAlreadyScheduled(shift.Day, shift.Line, shift.Time))
            {
                shift.Day = random.Next(0, dayMax);
                shift.Line = random.Next(0, lineMax);
                shift.Time = random.Next(0, timeMax);

                if (state.IsLeftOneShift())
                {
                    shift.Day = state.GetLastDay().Value;
                    shift.Time = state.GetLastTime().Value;
                    shift.Line = state.GetLastLine().Value;
                }
            }

            return shift;
        }

        private static int? SearchForDriver(ScheduleState state, int day, int lineNum)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            int driverMax = 11;

            int driver = random.Next(0, driverMax);

            int iterationToFind = 0;

            while ((
                       state.IsTodayDayOff((byte)(driver + 1), (byte)(day + 1)) == true
                       ||
                       state.IsDriverCanDriveLine((byte)(driver + 1), (byte)(lineNum + 1)) == false
                       ||
                       state.IsDriverBusyToday(day, driver) == true
                   )
                   &&
                   iterationToFind < 1000
            )
            {
                driver = random.Next(0, driverMax);
                iterationToFind++;
            }

            if (
                state.IsTodayDayOff((byte)(driver + 1), (byte)(day + 1)) == false
                &&
                state.IsDriverCanDriveLine((byte)(driver + 1), (byte)(lineNum + 1)) == true
                &&
                state.IsDriverBusyToday(day, driver) == false
                )
            {
                return driver;
            }
            else
            {
                return null;
            }
        }

        private static bool IsTransition(double probability)
        {
            Random randomizer = new Random(1);
            double value = randomizer.Next(0,1000) * 0.001;

            if (value < probability)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static double DecreaseTemperature(double initialTemperature, int iteration)
        {
            return initialTemperature * 0.1 / (iteration + 1);
        }

        private static double GetTransitionProbability(int diffEnergy, double temperature)
        {
            return Math.Exp((-1) *(diffEnergy / temperature));
        }

        static void PrintSchedule(ScheduleState state)
        {
            Console.WriteLine("Schedule");
            for (int j = 0; j < 11; j++)
            {
                Console.Write("Driver " + (int)(j+1) + ": ");
                if(j+1<10) Console.Write(" ");
                for (int i = 0; i < 14; i++)
                {
                    
                    for (int k = 0; k < 2; k++)
                    {
                        if (state.IsTodayDayOff((byte)(j + 1), (byte)(i + 1)))
                        {
                            Console.Write("x ");
                        }
                        else
                        {
                            if (state.IsPrefToWork((byte)(j+1),(byte)(i+1), (byte)k))
                            {
                                Console.BackgroundColor = ConsoleColor.Yellow;
                                Console.ForegroundColor = ConsoleColor.Black;
                            }

                            if (state.Schedule[i, j, k] != 0)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                            }

                            Console.Write(state.Schedule[i, j, k] + " ");

                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                    Console.Write(" ");
                }
                Console.WriteLine();
            }

        }

        static void PrintLeftShifts(byte[,,] result)
        {
            Console.WriteLine("Left shifts");

            
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 14; i++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        Console.Write(result[i,j,k]+" ");
                    }
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    }
}
