namespace QualificationTask.Model
{
    public class Server
    {
        public Server(int index, int size, int capacity)
        {
            Index = index;
            Size = size;
            Capacity = capacity;
        }

        public int Index { get; set; }
        public int Size { get; set; }
        public int Capacity { get; set; }

        public int Group { get; set; }
        public int Row { get; set; }
        public int Slot { get; set; }



        public int Score
        {
            get { return Capacity/Size; }
        }
    }
}
