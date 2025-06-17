namespace TradingNotifications.Application;

public static class Algorithms101
{
    // 🔍 Recherche linéaire
    public static int LinearSearch(int[] array, int target)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == target)
                return i;
        }
        return -1;
    }

    // 🔍 Recherche binaire (nécessite tableau trié)
    public static int BinarySearch(int[] sortedArray, int target)
    {
        int left = 0, right = sortedArray.Length - 1;
        while (left <= right)
        {
            int mid = (left + right) / 2;
            if (sortedArray[mid] == target) return mid;
            else if (sortedArray[mid] < target) left = mid + 1;
            else right = mid - 1;
        }
        return -1;
    }

    // 🔄 Tri à bulles
    public static void BubbleSort(int[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (array[j] > array[j + 1])
                {
                    int temp = array[j];
                    array[j] = array[j + 1];
                    array[j + 1] = temp;
                }
            }
        }
    }

    // 🧮 Factorielle récursive
    public static int Factorial(int n)
    {
        if (n <= 1) return 1;
        return n * Factorial(n - 1);
    }

    // 🧠 Vérifier un palindrome
    public static bool IsPalindrome(string word)
    {
        var reversed = new string(word.Reverse().ToArray());
        return word == reversed;
    }

    // 🧩 Vérifier si deux mots sont des anagrammes
    public static bool AreAnagrams(string a, string b)
    {
        var aSorted = new string(a.OrderBy(c => c).ToArray());
        var bSorted = new string(b.OrderBy(c => c).ToArray());
        return aSorted == bSorted;
    }

    // 🧾 Moyenne Mobile Simple (SMA)
    public static decimal SimpleMovingAverage(List<decimal> values, int period)
    {
        if (values.Count < period) return 0;
        return values.Skip(values.Count - period).Take(period).Average();
    }

    // 📈 Moyenne Mobile Exponentielle (EMA)
    public static decimal ExponentialMovingAverage(List<decimal> values, int period)
    {
        if (values.Count < period) return 0;

        decimal k = 2m / (period + 1);
        decimal ema = values.Take(period).Average();

        for (int i = period; i < values.Count; i++)
        {
            ema = (values[i] - ema) * k + ema;
        }

        return ema;
    }

    // 📉 MACD (EMA 12 - EMA 26)
    public static (decimal macd, decimal signal, decimal histogram) CalculateMACD(List<decimal> values)
    {
        if (values.Count < 26) return (0, 0, 0);

        var ema12 = ExponentialMovingAverage(values, 12);
        var ema26 = ExponentialMovingAverage(values, 26);
        var macd = ema12 - ema26;

        var macdSeries = new List<decimal>();
        for (int i = 0; i < values.Count - 8; i++)
        {
            var subList = values.Skip(i).Take(9).ToList();
            var shortEma = ExponentialMovingAverage(subList, 12);
            var longEma = ExponentialMovingAverage(subList, 26);
            macdSeries.Add(shortEma - longEma);
        }
        var signal = ExponentialMovingAverage(macdSeries, 9);
        var histogram = macd - signal;
        return (macd, signal, histogram);
    }

    // 📊 Bollinger Bands
    public static (decimal upper, decimal middle, decimal lower) CalculateBollingerBands(List<decimal> values, int period = 20)
    {
        if (values.Count < period) return (0, 0, 0);
        var slice = values.Skip(values.Count - period).Take(period).ToList();
        var mean = slice.Average();
        var std = (decimal)Math.Sqrt(slice.Select(v => (double)(v - mean) * (double)(v - mean)).Average());
        var upper = mean + 2 * std;
        var lower = mean - 2 * std;
        return (upper, mean, lower);
    }

    // 📈 RSI (Relative Strength Index)
    public static decimal CalculateRSI(List<decimal> closes, int period = 14)
    {
        if (closes.Count < period + 1) return 0;

        decimal gain = 0, loss = 0;
        for (int i = closes.Count - period; i < closes.Count; i++)
        {
            var delta = closes[i] - closes[i - 1];
            if (delta >= 0) gain += delta;
            else loss -= delta;
        }

        if (loss == 0) return 100;
        var rs = gain / loss;
        return 100 - (100 / (1 + rs));
    }

    // 📊 ADX (Average Directional Index) - version simplifiée
    public static decimal CalculateADX(List<decimal> high, List<decimal> low, List<decimal> close, int period = 14)
    {
        if (high.Count < period + 1 || low.Count < period + 1 || close.Count < period + 1)
            return 0;

        List<decimal> trList = new();
        for (int i = 1; i < high.Count; i++)
        {
            var tr = Math.Max((double)(high[i] - low[i]), Math.Max(
                Math.Abs((double)(high[i] - close[i - 1])),
                Math.Abs((double)(low[i] - close[i - 1]))));
            trList.Add((decimal)tr);
        }

        return trList.Skip(trList.Count - period).Average(); // Simplified: returning average TR as proxy for ADX
    }
}