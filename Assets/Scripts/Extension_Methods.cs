using System;

public static class Extension_Methods 
{
    public static bool IsEven(this int number) {
        return number % 2 == 0;
    }
    public static bool IsOdd(this int number) {
        return number % 2 != 0;
    }

   public static float RoundToNearestTen(this int number) {
      return (float)Math.Round(number / 10.0f) * 10;
   }
}