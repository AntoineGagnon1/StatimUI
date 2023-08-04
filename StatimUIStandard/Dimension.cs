using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public enum DimensionUnit { Pixel, Decimal, Auto }
    public class Dimension
    {
        public Dimension(float scalar, DimensionUnit unit)
        {
            _scalar = scalar;
            Unit = unit;
        }

        private float _scalar = 0f;
        public float Scalar 
        {
            get => _scalar;
            set
            {
                if (Unit == DimensionUnit.Auto)
                    _scalar = value;
            }
        }
        public DimensionUnit Unit { get; set; }

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
    }
}
