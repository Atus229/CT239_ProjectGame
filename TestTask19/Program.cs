using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== KIỂM THỬ RATING SAO (BR-04) ===");
        Console.WriteLine($"• 55s (Trong hạn 20-60s) : {CalculateStarRating(55f, 20f, 60f)} sao");
        Console.WriteLine($"• 64s (Trễ 4s < K=5s)    : {CalculateStarRating(64f, 20f, 60f)} sao");
        Console.WriteLine($"• 65s (Trễ 5s -> -1 sao)  : {CalculateStarRating(65f, 20f, 60f)} sao");
        Console.WriteLine($"• 85s (Trễ 25s -> min)   : {CalculateStarRating(85f, 20f, 60f)} sao");

        Console.WriteLine("\n=== KIỂM THỬ THƯỞNG VÀNG (BR-05) ===");
        Console.WriteLine($"• Giá trị 120, Hệ số x1.5 : {CalculateEarnedGold(120, 1.5f)} vàng");
    }

    static int CalculateStarRating(float totalJobTime, float tMin, float tMax, float timePenaltyK = 5f)
    {
        if (totalJobTime <= tMax) return 5;
        float overTime = totalJobTime - tMax;
        int penalty = (int)(overTime / timePenaltyK);
        return Math.Max(1, 5 - penalty);
    }

    static int CalculateEarnedGold(int totalItemValue, float rewardMultiplier)
    {
        return Math.Max(0, (int)(totalItemValue * rewardMultiplier));
    }
}