using System;
using System.Globalization;

[Serializable]
public struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>, IFormattable
{
    // 실제 값 = value * 10^exponent
    // 정규화 시 지수는 3의 배수로 유지한다.
    public double value;
    public int exponent;

    public BigNumber(int value) : this((double)value) { }

    public BigNumber(int value, int exponent)
        : this((double)value, exponent) { }

    public BigNumber(double value) : this(value, 0) { }

    public BigNumber(double value, int exponent)
    {
        this.value = value;
        this.exponent = exponent;

        Normalize();
    }

    private void Normalize()
    {
        // 무한대와 NaN은 정규화할 수 없으므로 거부한다.
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "BigNumber requires a finite value.");
        }

        if (value == 0)
        {
            exponent = 0;
            return;
        }

        int remainder = (exponent % 3 + 3) % 3;
        exponent = checked(exponent - remainder);

        // 큰 double에 10 또는 100을 곱할 때의 오버플로를 방지한다.
        while (Math.Abs(value) >= 1000)
        {
            value /= 1000;
            exponent = checked(exponent + 3);
        }

        value *= Math.Pow(10, remainder);

        while (Math.Abs(value) >= 1000)
        {
            value /= 1000;
            exponent = checked(exponent + 3);
        }

        while (Math.Abs(value) < 1)
        {
            value *= 1000;
            exponent = checked(exponent - 3);
        }
    }

    public bool IsZeroOrNegative => value <= 0;

    public override string ToString()
    {
        return ToString(null, CultureInfo.CurrentCulture);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        var number = new BigNumber(value, exponent);
        string[] units = { "", "K", "M", "B", "T" };

        string digits = number.value.ToString(
            string.IsNullOrEmpty(format) ? "0.##" : format,
            formatProvider);

        int unitIndex = number.exponent / 3;

        if (unitIndex >= 0 && unitIndex < units.Length)
        {
            return digits + units[unitIndex];
        }

        return digits + "e"
            + number.exponent.ToString(CultureInfo.InvariantCulture);
    }

    // 사칙 연산자
    public static BigNumber operator +(BigNumber a, BigNumber b)
    {
        if (a.value == 0)
            return b;

        if (b.value == 0)
            return a;

        // 지수가 큰 쪽을 기준으로 맞춘다.
        if (a.exponent > b.exponent)
        {
            double convertedValue = b.value
                * Math.Pow(10, (long)b.exponent - a.exponent);

            return new BigNumber(
                a.value + convertedValue,
                a.exponent);
        }

        double convertedA = a.value
            * Math.Pow(10, (long)a.exponent - b.exponent);

        return new BigNumber(
            b.value + convertedA,
            b.exponent);
    }

    public static BigNumber operator -(BigNumber a, BigNumber b)
    {
        return a + new BigNumber(-b.value, b.exponent);
    }

    public static BigNumber operator *(BigNumber a, BigNumber b)
    {
        if (a.value == 0 || b.value == 0)
            return new BigNumber(0);

        return new BigNumber(
            a.value * b.value,
            checked(a.exponent + b.exponent));
    }

    public static BigNumber operator /(BigNumber a, BigNumber b)
    {
        if (b.value == 0)
            throw new DivideByZeroException();

        if (a.value == 0)
            return new BigNumber(0);

        return new BigNumber(
            a.value / b.value,
            checked(a.exponent - b.exponent));
    }

    // 거듭제곱을 BigNumber 연산으로 계산한다.
    public static BigNumber Pow(BigNumber number, int power)
    {
        if (power < 0)
            throw new ArgumentOutOfRangeException(nameof(power));

        var result = new BigNumber(1);

        while (power > 0)
        {
            if ((power & 1) != 0)
            {
                result *= number;
            }

            power >>= 1;

            if (power > 0)
            {
                number *= number;
            }
        }

        return result;
    }

    public int CompareTo(BigNumber other)
    {
        var a = new BigNumber(value, exponent);
        var b = new BigNumber(other.value, other.exponent);

        int sign = Math.Sign(a.value);
        int otherSign = Math.Sign(b.value);

        if (sign != otherSign)
            return sign.CompareTo(otherSign);

        if (sign == 0)
            return 0;

        int order = a.exponent.CompareTo(b.exponent);

        return order != 0
            ? order * sign
            : a.value.CompareTo(b.value);
    }

    public bool Equals(BigNumber other)
    {
        return CompareTo(other) == 0;
    }

    public override bool Equals(object obj)
    {
        return obj is BigNumber other && Equals(other);
    }

    public override int GetHashCode()
    {
        var number = new BigNumber(value, exponent);

        return number.value.GetHashCode()
            ^ number.exponent.GetHashCode();
    }

    // 비교 연산자
    public static bool operator >(BigNumber a, BigNumber b)
    {
        return a.CompareTo(b) > 0;
    }

    public static bool operator <(BigNumber a, BigNumber b)
    {
        return a.CompareTo(b) < 0;
    }

    public static bool operator >=(BigNumber a, BigNumber b)
    {
        return a.CompareTo(b) >= 0;
    }

    public static bool operator <=(BigNumber a, BigNumber b)
    {
        return a.CompareTo(b) <= 0;
    }

    public static bool operator ==(BigNumber a, BigNumber b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(BigNumber a, BigNumber b)
    {
        return !a.Equals(b);
    }
}