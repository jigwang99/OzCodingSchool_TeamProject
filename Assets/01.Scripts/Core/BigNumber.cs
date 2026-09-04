using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct BigNumber
{
    //실제 숫자 앞부분
    public double value;
    //숫자가 얼마나 큰지 나타내는 지수
    public int exponent;

    //생성자 1
    public BigNumber(double value)
    {
        this.value = value;
        this.exponent = 0;

        Normalize();
    }

    //생성자 2
    public BigNumber(double value, int exponent)
    {
        this.value = value;
        this.exponent = exponent;

        Normalize();
    }

    private void Normalize()
    {
        if (value == 0)
        {
            exponent = 0;
            return;
        }

        while (Math.Abs(value) >= 1000)
        {
            value /= 1000;
            exponent += 3;
        }

        while (Math.Abs(value) < 1)
        {
            value *= 1000;
            exponent -= 3;
        }
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
}
