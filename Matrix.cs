namespace _3D_Renderer
{
    readonly struct Matrix
    {
        public Arr2D<double> Values { get; }
        public Matrix(Arr2D<double> Values)
        {
            this.Values = Values;
        }

        public int Width()
        {
            return Values.Width;
        }
        public int Height()
        {
            return Values.Height;
        }

        public Matrix Transpose()
        {
            Matrix Transpose = new Matrix(new Arr2D<double>(Height(), Width()));
            for (int x = 0; x < Height(); x++)
            {
                for (int y = 0; y < Width(); y++)
                {
                    Transpose.Values[y, x] = Values[x, y];
                }
            }
            return Transpose;
        }

        public Vec3D ToVec3D()
        {
            return new Vec3D(Values[0, 0], Values[1, 0], Values[2, 0]);
        }

        public Vec2D ToVec2D()
        {
            return new Vec2D(Values[0, 0], Values[1, 0]);
        }

        public static Matrix Multiply(Matrix MatrixA, Matrix MatrixB)
        {
            Matrix Product = new Matrix(new Arr2D<double>(MatrixB.Width(), MatrixB.Height()));
            for (int ProductX = 0; ProductX < Product.Height(); ProductX++)
            {
                for (int ProductY = 0; ProductY < Product.Width(); ProductY++)
                {
                    double Value = 0;
                    int MatrixAX = 0;
                    int MatrixBY = 0;
                    while (MatrixAX < MatrixA.Height() && MatrixBY < MatrixB.Width())
                    {
                        Value += MatrixA.Values[ProductY, MatrixAX] * MatrixB.Values[MatrixBY, ProductX];
                        MatrixAX++;
                        MatrixBY++;
                    }
                    Product.Values[ProductY, ProductX] = Value;
                }
            }
            return Product;
        }

        public static Matrix operator +(Matrix MatrixA, Matrix MatrixB)
        {
            Matrix Sum = new Matrix(new Arr2D<double>(MatrixB.Width(), MatrixB.Height()));
            for (int x = 0; x < Sum.Height(); x++)
            {
                for (int y = 0; y < Sum.Width(); y++)
                {
                    Sum.Values[x, y] = MatrixA.Values[x, y] + MatrixB.Values[x, y];
                }
            }
            return Sum;
        }
    }
}
