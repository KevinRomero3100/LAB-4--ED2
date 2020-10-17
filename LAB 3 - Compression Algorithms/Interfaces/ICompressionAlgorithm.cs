using System;
using System.Collections.Generic;
using System.Text;

namespace LAB_3___Compressor.Interfaces
{
    interface ICompressionAlgorithm
    {
        public byte[] EncodeData(byte[] content);
        public byte[] DecodeData(byte[] content);
        public double CompressionRatio();
        public double CompressionFactor();
        public double ReductionPercentage();

    }
}
