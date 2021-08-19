namespace _3D_Renderer
{
    partial class Double
    {
        //Some additional double functionality that is useful.
        public static double Lerp(double NumA, double NumB, double LerpFactor)
        {
            return NumA * (1 - LerpFactor) + NumB * LerpFactor;
        }
        public static double BiLerp(double NumA, double NumB, double NumC, double NumD, double LerpFactorA, double LerpFactorB)
        {
            //Could call Lerp three times but this is faster
            double Temp = LerpFactorA * LerpFactorB;
            return NumA * (Temp - LerpFactorA - LerpFactorB + 1) + NumB * (LerpFactorA - Temp) + NumC * (LerpFactorB - Temp) + NumD * (Temp);
        }
        public static double Mod(double NumA, double NumB)
        {
            //C sharp % is actually remainder, not modulus, confusingly enough
            return (NumA % NumB + NumB) % NumB;
        }
    }
}
