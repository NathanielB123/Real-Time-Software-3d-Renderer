using System;

namespace _3D_Renderer
{
    class JaggedArr<T>
    {
        // Just having an array of arrays works, but in the one place I am using jagged arrays, I needed to store positions of each row as well,
        // so having a class with that functionality built-in is helpful.
        private T[] Arr;

        private readonly int[] StartXs;
        private readonly int[] RunningIndexes;
        public int StartY { get; }
        public int Height { get { return StartXs.Length; } }

        public int Width(int Row)
        {
            return RunningIndexes[Row + 1] - RunningIndexes[Row];
        }

        public int StartX(int Row)
        {
            return StartXs[Row];
        }

        public JaggedArr(int StartY, int[] StartXs, int[] EndXs)
        {
            this.StartXs = StartXs;
            this.StartY = StartY;
            int ArrSize = 0;
            RunningIndexes = new int[StartXs.Length + 1];
            for (int RowNum = 0; RowNum < StartXs.Length; RowNum++)
            {
                RunningIndexes[RowNum] = ArrSize;
                ArrSize += EndXs[RowNum] - StartXs[RowNum];
            }
            RunningIndexes[StartXs.Length] = ArrSize;
            Arr = new T[ArrSize];
        }

        public void ReplaceRow(int y, T[] Data)
        {
            int RowNum = y - StartY;
            if (Data.Length == Width(RowNum))
            {
                Array.Copy(Data, 0, Arr, RunningIndexes[RowNum], Data.Length);
            }
            else
            {
                throw new Exception("Length of data does not match width of row!");
            }
        }

        public void ReplaceRows(JaggedArr<T> Data)
        {
            for (int RowNum = 0; RowNum < Data.Height; RowNum++)
            {
                ReplaceRow(Data.StartY + RowNum, Data.GetRow(Data.StartY + RowNum));
            }
        }

        public void Set(int x, int y, T Data)
        {
            int RowNum = y - StartY;
            Arr[x - StartXs[RowNum] + RunningIndexes[RowNum]] = Data;
        }

        public T Get(int x, int y)
        {
            int RowNum = y - StartY;
            return Arr[x - StartXs[RowNum] + RunningIndexes[RowNum]];
        }

        public T[] GetRow(int y)
        {
            int RowNum = y - StartY;
            T[] Row = new T[Width(RowNum)];
            Array.Copy(Arr, RunningIndexes[RowNum], Row, 0, Width(RowNum));
            return Row;
        }

        public T this[int x, int y]
        {
            get
            {
                return Get(x, y);
            }
            set
            {
                Set(x, y, value);
            }
        }
    }
}
