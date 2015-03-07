using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

        public static T Convert<T>(this string input)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                //Cast ConvertFromString(string text) : object to (T)
                return (T)converter.ConvertFromString(input);
            }
            return default(T);
        }

        public static T ChangeType<T>(this object obj)
        {
            return (T)System.Convert.ChangeType(obj, typeof(T));
        }

        public static IEnumerable<T> SplitAndParse<T>(this string input, char separator = ' ')
        {
            var splited = input.Split(separator);
            var result = splited.Select(Convert<T>);
            return result;
        }

        public static IEnumerable<T> ReadLineSplitAndParse<T>(this StreamReader reader, char separator = ' ')
        {
            var line = reader.ReadLine();
            return line.SplitAndParse<T>(separator);
        }

      
        //public static T[,] CreateMatrix<T>(this StreamReader reader,Size matrixSize , char columnSeparator = ' ')
        //{
        //    int i = 0, j = 0;
        //    var matrix = new T[matrixSize.Width, matrixSize.Height];

        //    var line = reader.ReadLine();

        //    while (!string.IsNullOrEmpty(line))
        //    {
        //        var cols =line.Split(columnSeparator);
        //        foreach (var col in cols)
        //        {
        //            matrix[i, j] = col.Convert<T>();
        //            j++;
        //        }
        //        i++;
        //    }

        //    return matrix;
        //}

        public static TMatrix[,] CreateMatrix<TMatrix, TVal>(this StreamReader reader, Size matrixSize, Func<int, int, TVal, TMatrix[,], TMatrix> matcher)
        {
            int i = 0, j = 0;
            var matrix = new TMatrix[matrixSize.Height, matrixSize.Width];

            var line = reader.ReadLine();

            while (!string.IsNullOrEmpty(line))
            {
                j = 0;
                foreach (var col in line)
                {
                    var val = col.ToString().Convert<TVal>();
                    matrix[i, j] = matcher(i,j,val,matrix);
                    j++;
                }
                i++;
                line = reader.ReadLine();
            }

            return matrix;
        }


    }
}
