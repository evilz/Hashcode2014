using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrialRound
{
    public static class Helpers
    {

        public static Stream AsStream(this string input)
        {
            // convert string to stream
            byte[] byteArray = Encoding.UTF8.GetBytes(input);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream stream = new MemoryStream(byteArray);
            return stream;
        }

        public static string GetString(this Stream stream)
        {
            // convert stream to string
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return text;
        }

        public static T ChangeType<T>(this object obj)
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static IEnumerable<T> SplitAndParse<T>(this string input, char separator = ' ')
        {
            var splited = input.Split(separator);
            var result = splited.Select(ChangeType<T>);
            return result;
        }

        public static IEnumerable<T> ReadLineSplitAndParse<T>(this StreamReader reader, char separator = ' ')
        {
            var line = reader.ReadLine();
            return line.SplitAndParse<T>(separator);
        }



    }
}
