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
            Random random = new Random((int)DateTime.Now.Ticks);

            int dayMax = 14;
            int driverMax = 11;
            int lineMax = 3; //TODO: maybe 3
            int timeMax = 2;

            int day = random.Next(0, dayMax);
            int driver = random.Next(0, driverMax);
            int time = random.Next(0, timeMax);
            int lineNum = random.Next(0, lineMax);

            while (state.IsShiftScheduled(day, lineNum, time)) // while not find not scheduled shift
            {
                day = random.Next(0, dayMax);
                lineNum = random.Next(0, lineMax);
                time = random.Next(0, timeMax);
            }

            bool driverHasDayOff = state.IsTodayDayOff((byte) (driver + 1), (byte) (day + 1));
            bool isDriverCanDriveTheLine =
                state.IsDriverCanDriveLine((byte) (driver + 1), (byte) (lineNum + 1)) == false;
            bool isDriverBusyToday = state.IsDriverBusyToday(day, driver, time);

            while (
                driverHasDayOff == true 
                || 
                isDriverCanDriveTheLine == false
                ||
                isDriverBusyToday == true
                )
            {
                driver = random.Next(0, driverMax);

                driverHasDayOff = state.IsTodayDayOff((byte)(driver + 1), (byte)(day + 1));
                isDriverCanDriveTheLine =
                    state.IsDriverCanDriveLine((byte)(driver + 1), (byte)(lineNum + 1)) ;
                isDriverBusyToday = state.IsDriverBusyToday(day, driver, time);
            }

            state.SetLineToDriver(lineNum,  driver, day,time);

            return state;
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

    }
}
