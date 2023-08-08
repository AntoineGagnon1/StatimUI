using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public enum DimensionUnit { Pixel, Decimal, Auto }
    public readonly struct Dimension
    {
        public Dimension(float scalar, DimensionUnit unit)
        {
            Scalar = scalar;
            Unit = unit;
        }

        public readonly float Scalar;

        public readonly DimensionUnit Unit;

        public float GetPixelSize(Component? parent)
        {
            switch (Unit)
            {
                case DimensionUnit.Pixel:
                case DimensionUnit.Auto: 
                    return Scalar;
                case DimensionUnit.Decimal: 
                    return Scalar * (parent?.PixelWidth ?? 0.0f);
            }
            throw new InvalidDataException($"Invalid SizeUnit value : {Unit}({(int)Unit})");
        }

        public Dimension WithScalar(float scalar) => new Dimension(scalar, Unit);
        public Dimension WithUnit(DimensionUnit unit) => new Dimension(Scalar, unit);
    }
}
