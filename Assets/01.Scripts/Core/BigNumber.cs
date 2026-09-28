using System;
using System.Globalization;

[Serializable]
public struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>, IFormattable
{
    // 실제 값 = (value / ScaleFactor) * 10^exponent
    public long value;
    public int exponent;

    private const long ScaleFactor = 100; // 소수점 둘째 자리까지 정밀도 유지

    public BigNumber(long rawValue, int exponent)
    {
        this.value = rawValue;
        this.exponent = exponent;
        Normalize();
    }

    public BigNumber(long integerValue)
    {
        this.value = integerValue * ScaleFactor;
        this.exponent = 0;
        Normalize();
    }

    public BigNumber(double doubleValue)
    {
        if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
        {
            throw new ArgumentOutOfRangeException(nameof(doubleValue), "BigNumber requires a finite value.");
        }

        if (doubleValue == 0)
        {
            this.value = 0;
            this.exponent = 0;
            return;
        }

        int exp = 0;
        while (Math.Abs(doubleValue) >= 10.0)
        {
            doubleValue /= 10.0;
            exp++;
        }
        while (Math.Abs(doubleValue) < 1.0 && doubleValue != 0)
        {
            doubleValue *= 10.0;
            exp--;
        }

        this.value = (long)Math.Round(doubleValue * ScaleFactor);
        this.exponent = exp;
        Normalize();
    }

    public BigNumber(double val, int exp)
    {
        // double 값을 받아 초기화할 때 지수와 값을 정확히 반영
        double total = val * Math.Pow(10, exp);
        int targetExp = 0;

        if (total != 0)
        {
            targetExp = (int)Math.Floor(Math.Log10(Math.Abs(total)));
            val = total / Math.Pow(10, targetExp);
        }

        this.value = (long)Math.Round(val * ScaleFactor);
        this.exponent = targetExp;
        Normalize();
    }

    private void Normalize()
    {
        if (value == 0)
        {
            exponent = 0;
            return;
        }

        // 지수를 항상 3의 배수로 맞춤 (방치형 게임 표준 단위 K, M, B...)
        int remainder = exponent % 3;
        if (remainder != 0)
        {
            exponent -= remainder;
            value = (long)Math.Round(value * Math.Pow(10, remainder));
        }

        // value 절댓값이 너무 크면 지수를 올림
        while (Math.Abs(value) >= ScaleFactor * 1000)
        {
            value /= 1000;
            exponent = checked(exponent + 3);
        }

        // value 절댓값이 너무 작고 지수가 남아있으면 지수를 내림
        while (Math.Abs(value) < ScaleFactor && exponent >= 3)
        {
            value *= 1000;
            exponent = checked(exponent - 3);
        }

        if (value == 0)
        {
            exponent = 0;
        }
    }

    public bool IsZeroOrNegative => value <= 0;

    public override string ToString()
    {
        return ToString(null, CultureInfo.CurrentCulture);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        string[] units = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc", "Ud", "Dd", "Td" };

        int unitIndex = exponent / 3;
        double realValue = (double)value / ScaleFactor;

        string digits;
        if (unitIndex == 0)
        {
            digits = Math.Round(realValue).ToString("0", formatProvider);
        }
        else
        {
            digits = realValue.ToString(string.IsNullOrEmpty(format) ? "0.##" : format, formatProvider);
        }

        if (unitIndex >= 0 && unitIndex < units.Length)
        {
            return digits + units[unitIndex];
        }

        return digits + "e" + exponent.ToString(CultureInfo.InvariantCulture);
    }

    // 사칙 연산자
    public static BigNumber operator +(BigNumber a, BigNumber b)
    {
        if (a.value == 0) return b;
        if (b.value == 0) return a;

        if (a.exponent > b.exponent)
        {
            long diff = a.exponent - b.exponent;
            long shiftedB = (long)Math.Round(b.value / Math.Pow(10, diff));
            return new BigNumber(a.value + shiftedB, a.exponent);
        }
        else if (b.exponent > a.exponent)
        {
            long diff = b.exponent - a.exponent;
            long shiftedA = (long)Math.Round(a.value / Math.Pow(10, diff));
            return new BigNumber(b.value + shiftedA, b.exponent);
        }
        else
        {
            return new BigNumber(a.value + b.value, a.exponent);
        }
    }

    public static BigNumber operator -(BigNumber a, BigNumber b)
    {
        if (a.exponent == b.exponent && a.value == b.value)
        {
            return new BigNumber(0);
        }

        bool isNegative = false;
        if (a < b)
        {
            var temp = b - a;
            return new BigNumber(-temp.value, temp.exponent);
        }

        int expDiff = a.exponent - b.exponent;

        if (expDiff > 18) return a;

        long baseValueA = a.value;
        long baseValueB = b.value;

        if (expDiff > 0)
        {
            long divisor = (long)Math.Pow(10, expDiff);

            baseValueB = baseValueB / divisor;
        }
        else if (expDiff < 0)
        {
            int diff = -expDiff;
            long multiplier = (long)Math.Pow(10, diff);
            baseValueA *= multiplier;
        }

        long newValue = baseValueA - baseValueB;
        int newExp = a.exponent;

        return new BigNumber(newValue, newExp);
    }

    public static BigNumber operator *(BigNumber a, BigNumber b)
    {
        if (a.value == 0 || b.value == 0) return new BigNumber(0);

        long newVal = (a.value * b.value) / ScaleFactor;
        int newExp = checked(a.exponent + b.exponent);
        return new BigNumber(newVal, newExp);
    }

    public static BigNumber operator /(BigNumber a, BigNumber b)
    {
        if (b.value == 0) throw new DivideByZeroException();
        if (a.value == 0) return new BigNumber(0);

        long newVal = (a.value * ScaleFactor) / b.value;
        int newExp = checked(a.exponent - b.exponent);
        return new BigNumber(newVal, newExp);
    }

    public static BigNumber Pow(BigNumber number, int power)
    {
        if (power < 0) throw new ArgumentOutOfRangeException(nameof(power));
        var result = new BigNumber(1);

        while (power > 0)
        {
            if ((power & 1) != 0) result *= number;
            power >>= 1;
            if (power > 0) number *= number;
        }
        return result;
    }

    public int CompareTo(BigNumber other)
    {
        if (exponent == other.exponent)
        {
            return value.CompareTo(other.value);
        }

        double aReal = ((double)value / ScaleFactor) * Math.Pow(10, exponent);
        double bReal = ((double)other.value / ScaleFactor) * Math.Pow(10, other.exponent);
        return aReal.CompareTo(bReal);
    }

    public bool Equals(BigNumber other) => CompareTo(other) == 0;
    public override bool Equals(object obj) => obj is BigNumber other && Equals(other);
    public override int GetHashCode() => value.GetHashCode() ^ exponent.GetHashCode();

    // 비교 연산자
    public static bool operator >(BigNumber a, BigNumber b) => a.CompareTo(b) > 0;
    public static bool operator <(BigNumber a, BigNumber b) => a.CompareTo(b) < 0;
    public static bool operator >=(BigNumber a, BigNumber b) => a.CompareTo(b) >= 0;
    public static bool operator <=(BigNumber a, BigNumber b) => a.CompareTo(b) <= 0;
    public static bool operator ==(BigNumber a, BigNumber b) => a.Equals(b);
    public static bool operator !=(BigNumber a, BigNumber b) => !a.Equals(b);
}