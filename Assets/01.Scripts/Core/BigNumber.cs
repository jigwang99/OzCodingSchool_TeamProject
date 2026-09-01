using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct BigNumber
{
    public double value;
    public int exponent;

    public BigNumber(double value)
    {
        this.value = value;
        this.exponent = 0;

        Normalize();
    }

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
}
