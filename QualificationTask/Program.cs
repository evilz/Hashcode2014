using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QualificationTask.Model;
using TrialRound.Extentions;

namespace QualificationTask
{
    class Program
    {
        private static void Main(string[] args)
        {
            const string FILE_NAME = @"Samples/tiny.in";

            StringBuilder sbOut = new StringBuilder();

            var rowsCount = 0;
            var slotsCount = 0;
            var slotUnavailableCount = 0;
            var poolCount = 0;
            var serversCount = 0;
            List<Server> servers;
            IEnumerable<int[]> unuvailable;

            using (var inputStream = File.OpenRead(FILE_NAME))
            {
                var reader = new StreamReader(inputStream);

                // parse first line to get Matrix Size
                var inputInt = reader.ExtractValues<int>().ToArray();

                 rowsCount = inputInt[0];
                 slotsCount = inputInt[1];
                 slotUnavailableCount = inputInt[2];
                 poolCount = inputInt[3];
                 serversCount = inputInt[4];

                 unuvailable = (from i in Enumerable.Range(0, slotUnavailableCount)
                                   select reader.ExtractValues<int>().ToArray())
                                   .ToList();

                 servers = (from i in Enumerable.Range(0, serversCount)
                               select reader.ExtractValues<int>().ToArray())
                              .Select(x => new Server(IndexGenerator.GetIndex(), x[0], x[1]))
                              .ToList();


                

                reader.Close();
                inputStream.Close();
            }

                int currentSlot = 0;
                int currentRow = 0;
                int currentCpuInRow = 0;
                int cpu = 0;

                int nbServerByGroup = serversCount / poolCount;

            int currentGroup = 0;

            // VB
                foreach (var server in servers.OrderByDescending(s=>s.Score))
                {
                    

                    if (currentSlot + server.Size > slotsCount) // Slot limit reached
                    {
                        currentSlot = 0;
                        currentCpuInRow = 0;
                        currentRow++;
                    }



                    // VB
                    if (currentGroup == poolCount)
                        poolCount = 0;

                    if (currentRow >= rowsCount)
                    {
                        Console.WriteLine("x");
                        continue;
                    }

                    // VB
                    server.Row = currentRow;
                    server.Group = 0;
                    server.Slot = currentSlot;
                    currentGroup++;

                    // filling current slot
                   // Console.WriteLine("{0} {1} {2}", currentRow, currentSlot, server.Index / nbServerByGroup);

                    currentSlot += server.Size;
                    currentCpuInRow += server.Capacity;

                }

            foreach (var server in servers.OrderBy(s => s.Index))
            {
                Console.WriteLine("{0} {1} {2}", server.Row, server.Slot, server.Group);
            }



            Console.ReadLine();
        }

    }
}