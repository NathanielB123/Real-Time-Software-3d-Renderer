namespace _3D_Renderer
{
    class Arr2D<T>
    {
        //I have found in testing, for whatever reason, 1d array indexing is significantly faster than 2d array indexing in c# so I have decided to 
        //implement my own 2d array class for better performance
        private T[] Arr;
        public int Width { get; }
        public int Height { get; }

        public Arr2D(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            Arr = new T[Width * Height];
        }

        public Arr2D(T[,] PrevArr)
        {
            Width = PrevArr.GetLength(0);
            Height = PrevArr.GetLength(1);
            Arr = new T[Width * Height];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Set(x, y, PrevArr[x, y]);
                }
            }
        }

        public Arr2D(T[] PrevArr, int Width, int Height)
        {
            Arr = PrevArr;
            this.Width = Width;
            this.Height = Height;
        }

        public void Set(int x, int y, T Data)
        {
            Arr[x + y * Width] = Data;
        }

        public T Get(int x, int y)
        {
            return Arr[x + y * Width];
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

        public static Arr2D<T> operator +(Arr2D<T> ArrA, Arr2D<T> ArrB)
        {
            T[] NewArr = new T[ArrA.Width * ArrA.Height + ArrB.Width * ArrB.Height];
            ArrA.Arr.CopyTo(NewArr, 0);
            ArrB.Arr.CopyTo(NewArr, ArrA.Width * ArrA.Height);
            return new Arr2D<T>(NewArr, ArrA.Width, ArrA.Height + ArrB.Height);
        }

        public Arr2D<T> Resize(int NewWidth, int NewHeight)
        {
            //Definitely could be made faster, but shouldn't be called very often
            Arr2D<T> NewArr = new Arr2D<T>(NewWidth, NewHeight);
            for (int x = 0; x < NewWidth; x++)
            {
                for (int y = 0; y < NewWidth; y++)
                {
                    NewArr[x, y] = this[x, y];
                }
            }
            return NewArr;
        }
    }
}
