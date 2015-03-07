
using System;

namespace TrialRound
{
    public class ColoredConsoleLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
        
        public void Log(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format,args));
        }
    }
}