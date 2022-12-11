using System.Numerics;

namespace EmmaWords;

public static class PrimeTest
{
    public static bool IsPrime(uint n)
    {
        if (n < 2) return false;
        if (n == 2 || n == 3 || n == 5 || n == 7) return true;
        if (n % 2 == 0) return false;

        var n1 = n - 1;
        var r = 1;
        var d = n1;
        while (d % 2 == 0)
        {
            r++;
            d >>= 1;
        }
        if (!Witness(2, r, d, n, n1)) return false;
        if (n < 2047) return true;
        return Witness(7, r, d, n, n1)
               && Witness(61, r, d, n, n1);
    }

    // a single instance of the Miller-Rabin Witness loop, optimized for odd numbers < 2e32
    private static bool Witness(int a, int r, uint d, uint n, uint n1)
    {
        var x = ModPow((ulong)a, d, n);
        if (x == 1 || x == n1) return true;

        while (r > 1)
        {
            x = ModPow(x, 2, n);
            if (x == 1) return false;
            if (x == n1) return true;
            r--;
        }
        return false;
    }
    static uint ModPow(ulong value, uint exponent, uint modulus)
    {
        //value %= modulus; // unnecessary here because we know this is true every time already
        ulong result = 1;
        while (exponent > 0)
        {
            if ((exponent & 1) == 1) result = result * value % modulus;
            value = value * value % modulus;
            exponent >>= 1;
        }
        return (uint)result;
    }

    public static bool IsPrime(ulong n)
    {
        if (n <= uint.MaxValue) return IsPrime((uint)n);
        if (n % 2 == 0) return false;

        BigInteger bn = n; // converting to BigInteger here to avoid converting up to 48 times below
        var n1 = bn - 1;
        var r = 1;
        var d = n1;
        while (d.IsEven)
        {
            r++;
            d >>= 1;
        }
        if (!Witness(2, r, d, bn, n1)) return false;
        if (!Witness(3, r, d, bn, n1)) return false;
        if (!Witness(5, r, d, bn, n1)) return false;
        if (!Witness(7, r, d, bn, n1)) return false;
        if (!Witness(11, r, d, bn, n1)) return false;
        if (n < 2152302898747) return true;
        if (!Witness(13, r, d, bn, n1)) return false;
        if (n < 3474749660383) return true;
        if (!Witness(17, r, d, bn, n1)) return false;
        if (n < 341550071728321) return true;
        if (!Witness(19, r, d, bn, n1)) return false;
        if (!Witness(23, r, d, bn, n1)) return false;
        if (n < 3825123056546413051) return true;
        return Witness(29, r, d, bn, n1)
               && Witness(31, r, d, bn, n1)
               && Witness(37, r, d, bn, n1);
    }

    // a single instance of the Miller-Rabin Witness loop
    private static bool Witness(BigInteger a, int r, BigInteger d, BigInteger n, BigInteger n1)
    {
        var x = BigInteger.ModPow(a, d, n);
        if (x == BigInteger.One || x == n1) return true;

        while (r > 1)
        {
            x = BigInteger.ModPow(x, 2, n);
            if (x == BigInteger.One) return false;
            if (x == n1) return true;
            r--;
        }
        return false;
    }

    // for comparison
    public static bool NaiveIsPrime(uint n)
    {
        if (n == 2 || n == 3 || n == 5 || n == 7) return true;
        if (n % 2 == 0) return false;
        if (n <= 10) return false;
        int l = (int)Math.Ceiling(Math.Sqrt(n)) + 1;
        for (int x = 3; x < l; x += 2)
        {
            if (n % x == 0) return false;
        }
        return true;
    }
}