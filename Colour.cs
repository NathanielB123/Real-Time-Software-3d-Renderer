using System;

namespace _3D_Renderer
{
    readonly struct Colour
    {
        //Colours will never be out of the range 0-255 and bytes take up significantly less memory than integers
        public byte Red { get; }
        public byte Blue { get; }
        public byte Green { get; }
        public byte Alpha { get; }
        public Colour(int Red, int Green, int Blue, int Alpha)
        {
            if (Red > 255) { this.Red = 255; }
            else if (Red < 0) { this.Red = 0; }
            else { this.Red = (byte)Red; }

            if (Green > 255) { this.Green = 255; }
            else if (Green < 0) { this.Green = 0; }
            else { this.Green = (byte)Green; }

            if (Blue > 255) { this.Blue = 255; }
            else if (Blue < 0) { this.Blue = 0; }
            else { this.Blue = (byte)Blue; }

            if (Alpha > 255) { this.Alpha = 255; }
            else if (Alpha < 0) { this.Alpha = 0; }
            else { this.Alpha = (byte)Alpha; }
        }
        public Colour(int Red, int Green, int Blue)
        {
            if (Red > 255) { this.Red = 255; }
            else if (Red < 0) { this.Red = 0; }
            else { this.Red = (byte)Red; }

            if (Green > 255) { this.Green = 255; }
            else if (Green < 0) { this.Green = 0; }
            else { this.Green = (byte)Green; }

            if (Blue > 255) { this.Blue = 255; }
            else if (Blue < 0) { this.Blue = 0; }
            else { this.Blue = (byte)Blue; }

            Alpha = 255;
        }

        //Returns in form BGR as that is the order used in the bytes of the bitmap image when the colour is written
        public int this[int Index]
        {
            get
            {
                if (Index == 0) { return Blue; }
                else if (Index == 1) { return Green; }
                else if (Index == 2) { return Red; }
                else if (Index == 3) { return Alpha; }
                else { return 0; }
            }
        }

        public static Colour Lerp(Colour ColA, Colour ColB, double LerpFactor)
        {
            return ColA.ScalarMult(1 - LerpFactor) + ColB.ScalarMult(LerpFactor);
        }

        public static Colour BiLerp(Colour ColourA, Colour ColourB, Colour ColourC, Colour ColourD, double LerpFactorA, double LerpFactorB)
        {
            //Could call Lerp three times but this is faster
            double Temp = LerpFactorA * LerpFactorB;
            return ColourA.ScalarMult(Temp - LerpFactorA - LerpFactorB + 1) + ColourB.ScalarMult(LerpFactorA - Temp) + ColourC.ScalarMult(LerpFactorB - Temp) + ColourD.ScalarMult(Temp);
        }

        public Colour ScalarMult(double Scalar)
        {
            return new Colour((int)Math.Round(Red * Scalar), (int)Math.Round(Green * Scalar), (int)Math.Round(Blue * Scalar), (int)Math.Round(Alpha * Scalar));
        }

        public static Colour operator +(Colour ColA, Colour ColB)
        {
            return new Colour(ColA.Red + ColB.Red, ColA.Green + ColB.Green, ColA.Blue + ColB.Blue, ColA.Alpha + ColB.Alpha);
        }

        public static Colour operator -(Colour ColA, Colour ColB)
        {
            return new Colour(ColA.Red - ColB.Red, ColA.Green - ColB.Green, ColA.Blue - ColB.Blue, ColA.Alpha - ColB.Alpha);
        }

        public static Colour ComponentWiseProd(Colour ColA, Colour ColB)
        {
            return new Colour((int)Math.Round((double)ColA.Red * ColB.Red / 255.0),
                              (int)Math.Round((double)ColA.Green * ColB.Green / 255.0),
                              (int)Math.Round((double)ColA.Blue * ColB.Blue / 255.0),
                              (int)Math.Round((double)ColA.Alpha * ColB.Alpha / 255.0));
        }
    }
}
