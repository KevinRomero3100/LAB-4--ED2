using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LAB_3___Compressor.LZW_Commpression
{
    public class LZW : Interfaces.ICompressionAlgorithm
    {

        Dictionary<int, List<byte>> principalCodesData = new Dictionary<int, List<byte>>();
        Dictionary<int, List<byte>> secondaryCodesData = new Dictionary<int, List<byte>>();


        double original_weight;
        double compressed_weight;

        public List<int> firstCompresion = new List<int>();

        public double ReductionPercentage()
        {
            return 1 - (compressed_weight / original_weight);
        }

        public double CompressionFactor()
        {
            return (original_weight / compressed_weight);
        }

        public double CompressionRatio()
        {
            return (compressed_weight / original_weight);
        }

      
        public byte[] EncodeData(byte[] content)
        {
            List<byte> cadenaByte = new List<byte>();
            foreach (var item in content)
            {
                cadenaByte = new List<byte>();
                cadenaByte.Add(item);

                if (!ContainsValue(cadenaByte, principalCodesData))
                {
                    principalCodesData.Add( getKey(0),cadenaByte);
                }
            }
            cadenaByte = new List<byte>();
            return EncodeData(content, cadenaByte, 0);
        }  

        byte[] EncodeData(byte[] content, List<byte> cadenaByte, int posByte)
        {
           
            var item = content[posByte];
            cadenaByte.Add(item);

            if (posByte == content.Length)
            {
                return SecondCompression();
            }
            if (ContainsValue(cadenaByte, principalCodesData) || ContainsValue(cadenaByte, secondaryCodesData))
            {
                posByte = posByte + 1;
                if (content.Length > posByte)
                {
                    EncodeData(content, cadenaByte, posByte);
                }
            }
            else
            {
                secondaryCodesData.Add(getKey(1),cadenaByte);
                FistCompression(cadenaByte);
                cadenaByte = new List<byte>();
                EncodeData(content, cadenaByte, posByte);
            }
            return content;
        }

        byte[] SecondCompression()
        {
            List<byte> finalCompression = new List<byte>();
            string binaryCode = "";
            ConvertDictionary(finalCompression);
            var binaryMax = Convert.ToString(firstCompresion.Max()).Length;

            foreach (var item in firstCompresion)
            {
                var newBinary = Convert.ToString(item, 2);
                if ( newBinary.Length < binaryMax)
                {
                    //Balance de 0 faltantes para llagar al maximo re querido
                    while (newBinary.Length == binaryMax)
                    {
                        newBinary = "0" + newBinary;
                    }
                    binaryCode += newBinary;
                }
            }
            var splitSize = 8;

            for (int i = 0; i < binaryCode.Length; i += splitSize)
            {
                if (i + splitSize > binaryCode.Length)
                {
                    splitSize = binaryCode.Length - i;
                    var split = binaryCode.Substring(i, splitSize);
                    //Balance de 0 faltantes para llagar al maximo re querido
                    while (split.Length == binaryMax)
                    {
                        split = split + "0";
                    }
                    var by = Convert.ToByte(split, 2);
                    finalCompression.Add(by);
                }
                else
                {
                    var by = Convert.ToByte(binaryCode.Substring(i, splitSize), 2);
                    finalCompression.Add(by);
                }
            }
            return finalCompression.ToArray();
        }

        void ConvertDictionary(List<byte> finalCompression)
        {
            foreach (var item in principalCodesData)
            {
                finalCompression.Add(Convert.ToByte(item));
            }
        }
        void FistCompression(List<byte> cadenaByte)
        {
            if (cadenaByte.Count > 1)
            {
                cadenaByte.RemoveAt(cadenaByte.Count - 1);
                if (ContainsValue(cadenaByte, principalCodesData))
                {
                    int key = principalCodesData.FirstOrDefault(x => x.Value == cadenaByte).Key;
                    firstCompresion.Add(key);
                }
                else if (ContainsValue(cadenaByte, secondaryCodesData))
                {
                    int key = secondaryCodesData.FirstOrDefault(x => x.Value == cadenaByte).Key;
                    firstCompresion.Add(key);
                }
            }
        }

        int getKey(int indicador)
        {
            if (indicador == 1)
                return principalCodesData.Count + secondaryCodesData.Count + 1;
            else if (indicador == 0 ) return principalCodesData.Count + 1;
            else throw new NotImplementedException();
        }

        bool ContainsValue(List<byte> cadenaByte, Dictionary<int, List<byte>> dictionary)
        {
            for (int i = 1; i <= dictionary.Count(); i++)
            {
                var item = dictionary[i];
                if (item.SequenceEqual(cadenaByte)) return true;
            }
            return false;
        }

        public byte[] DecodeData(byte[] content)
        {
            throw new NotImplementedException();
        }

    }
}
