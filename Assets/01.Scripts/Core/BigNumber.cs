using System;
using System.Globalization;

[Serializable]
public struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>, IFormattable
{
    //실제 숫자 앞부분
    public double value;
    //숫자가 얼마나 큰지 나타내는 지수
    public int exponent;

    //생성자 1-1
    public BigNumber(int value)
    {
        this.value = value;
        this.exponent = 0;

        Normalize();
    }

    //생성자 1-2
    public BigNumber(int value, int exponent)
    {
        this.value = value;
        this.exponent = exponent;

        Normalize();
    }

    //생성자 2-1
    public BigNumber(double value)
    {
        this.value = value;
        this.exponent = 0;

        Normalize();
    }

    //생성자 2-2
    public BigNumber(double value, int exponent)
    {
        this.value = value;
        this.exponent = exponent;
        Normalize();
    }

    private void Normalize()
    {
        // 무한대는 아래 정규화 루프가 끝나지 않으므로 입력 단계에서 거부한다.
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value), "BigNumber requires a finite value.");
        if (value == 0)
        {
            exponent = 0;
            return;
        }

        int remainder = (exponent % 3 + 3) % 3;
        exponent = checked(exponent - remainder);
        // 먼저 가수를 정규화하여 큰 double에 10/100을 곱할 때의 오버플로를 방지한다.
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

    public override string ToString() => ToString(null, CultureInfo.CurrentCulture);

    public string ToString(string format, IFormatProvider formatProvider)
    {
        var number = new BigNumber(value, exponent);
        string[] units = { "", "K", "M", "B", "T" };
        string digits = number.value.ToString(string.IsNullOrEmpty(format) ? "0.##" : format, formatProvider);
        int unitIndex = number.exponent / 3;
        if (unitIndex >= 0 && unitIndex < units.Length)
            return digits + units[unitIndex];
        return digits + "e" + number.exponent.ToString(CultureInfo.InvariantCulture);
    }

    public static BigNumber operator +(BigNumber a, BigNumber b)
    {
        if (a.value == 0) return b;
        if (b.value == 0) return a;
        if (a.exponent > b.exponent)
            return new BigNumber(a.value + b.value * Math.Pow(10, (long)b.exponent - a.exponent), a.exponent);
        return new BigNumber(b.value + a.value * Math.Pow(10, (long)a.exponent - b.exponent), b.exponent);
    }

    public static BigNumber operator -(BigNumber a, BigNumber b)
    {
        return a + new BigNumber(-b.value, b.exponent);
    }

    public static BigNumber operator *(BigNumber a, BigNumber b)
    {
        if (a.value == 0 || b.value == 0) return new BigNumber(0);
        return new BigNumber(a.value * b.value, checked(a.exponent + b.exponent));
    }

    public static BigNumber operator /(BigNumber a, BigNumber b)
    {
        if (b.value == 0) throw new DivideByZeroException();
        if (a.value == 0) return new BigNumber(0);
        return new BigNumber(a.value / b.value, checked(a.exponent - b.exponent));
    }

    // 비용 증가율을 double로 먼저 계산하지 않아 높은 강화 레벨에서도 오버플로하지 않는다.
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
        var a = new BigNumber(value, exponent);
        var b = new BigNumber(other.value, other.exponent);
        int sign = Math.Sign(a.value);
        int otherSign = Math.Sign(b.value);
        if (sign != otherSign) return sign.CompareTo(otherSign);
        if (sign == 0) return 0;
        int order = a.exponent.CompareTo(b.exponent);
        return order != 0 ? order * sign : a.value.CompareTo(b.value);
    }

    public override string ToString()
    {
        string[] units =
        {
            "",
            "K",
            "M",
            "B",
            "T"
        };

        int unitIndex = exponent / 3;

        if (unitIndex < units.Length)
        {
            //소수점 최대 2자리까지 보여줌 -> 0.##
            return $"{value:0.##}{units[unitIndex]}";
        }
        //T를 넘겼을 때 일단 e로 표현 -> 나중에 필요하면 추가 예정
        return $"{value:0.##}e{exponent}";
    }

    //사칙 연산자
    public static BigNumber operator +(BigNumber a, BigNumber b)
    {
        if (a.value == 0)
            return b;

        if (b.value == 0)
            return a;

        // 지수가 큰 쪽을 기준으로 맞춤
        if (a.exponent > b.exponent)
        {
            double convertedValue = b.value * Math.Pow(10, b.exponent - a.exponent);

            return new BigNumber(a.value + convertedValue, a.exponent);
        }
        else
        {
            double convertedValue = a.value * Math.Pow(10, a.exponent - b.exponent);

            return new BigNumber(b.value + convertedValue, b.exponent);
        }
    }

    public static BigNumber operator -(BigNumber a, BigNumber b)
    {
        if (a.exponent > b.exponent)
        {
            double convertedValue = b.value * Math.Pow(10, b.exponent - a.exponent);

            return new BigNumber(a.value - convertedValue, a.exponent);
        }
        else
        {
            double convertedValue = a.value * Math.Pow(10, a.exponent - b.exponent);

            return new BigNumber(convertedValue - b.value, b.exponent);
        }
    }

    public static BigNumber operator *(BigNumber a, BigNumber b)
    {
        //value끼리 곱하고 exponent끼리 더하기
        return new BigNumber(a.value * b.value, a.exponent + b.exponent);
    }

    public static BigNumber operator /(BigNumber a, BigNumber b)
    {
        if (b.value == 0)
            throw new DivideByZeroException();

        return new BigNumber(a.value / b.value, a.exponent - b.exponent);
    }

    //비교 연산자
    public bool IsZeroOrNegative => value <= 0;

    // >
    public static bool operator >(BigNumber a, BigNumber b)
    {
        if (a.exponent != b.exponent)
            return a.exponent > b.exponent;

        return a.value > b.value;
    }

    // <
    public static bool operator <(BigNumber a, BigNumber b)
    {
        if (a.exponent != b.exponent)
            return a.exponent < b.exponent;

        return a.value < b.value;
    }

    // >=
    public static bool operator >=(BigNumber a, BigNumber b)
    {
        if (a.exponent != b.exponent)
            return a.exponent > b.exponent;

        return a.value >= b.value;
    }

    // <=
    public static bool operator <=(BigNumber a, BigNumber b)
    {
        if (a.exponent != b.exponent)
            return a.exponent < b.exponent;

        return a.value <= b.value;
    }

    // ==
    public static bool operator ==(BigNumber a, BigNumber b)
    {
        return a.exponent == b.exponent &&
               a.value == b.value;
    }

    // !=
    public static bool operator !=(BigNumber a, BigNumber b)
    {
        return !(a == b);
    }
}
