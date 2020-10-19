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


        int count_chars;
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
            count_chars = content.Length;
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

            for (int i = 0; i < content.Length; i++)
            {
                var item = content[i];
                cadenaByte.Add(item);
                if (!ContainsValue(cadenaByte, principalCodesData) && !ContainsValue(cadenaByte, secondaryCodesData))
                {
                    secondaryCodesData.Add(getKey(1), cadenaByte);
                    FistCompression(cadenaByte);
                    cadenaByte = new List<byte>();
                    i--;
                }
            }
            if (ContainsValue(cadenaByte,principalCodesData))
                firstCompresion.Add(KeyValue(cadenaByte, principalCodesData));
            else
            firstCompresion.Add(KeyValue(cadenaByte, secondaryCodesData));
            cadenaByte = new List<byte>();
            //EncodeData(content, cadenaByte, 0);

            original_weight = content.Length;
            return SecondCompression();
        }  

        void EncodeData(byte[] content, List<byte> cadenaByte, int posByte)
        {
            var item = content[posByte];
            cadenaByte.Add(item);
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
                secondaryCodesData.Add(getKey(1), cadenaByte);
                FistCompression(cadenaByte);
                cadenaByte = new List<byte>();
                EncodeData(content, cadenaByte, posByte);
            }
            return;
        }

        byte[] SecondCompression()
        {
            List<byte> finalCompression = new List<byte>();
            string binaryCode = "";
            var binaryMax = Convert.ToString(firstCompresion.Max(),2).Length;
            int count = 0;
            foreach (var item in firstCompresion)
            {
                var newBinary = Convert.ToString(item, 2);
                if ( newBinary.Length <= binaryMax)
                {
                    count++;
                    //Balance de 0 faltantes para llagar al maximo re querido
                    while (newBinary.Length != binaryMax)
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
                    while (split.Length < 8)
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
            //AddMetaData(finalCompression, binaryMax);
            compressed_weight = finalCompression.Count;
            return finalCompression.ToArray();
        }

        void AddMetaData(List<byte> finalCompression, int longitudDeBits)
        {
            var keys = principalCodesData.Keys.ToList();

            if (keys.Count > 0)
            {
                for (int i = keys.Count ; i > 0; i--)
                {
                    var item = principalCodesData[i];

                    finalCompression.Insert(0,Convert.ToByte(item[0]));
                }
                finalCompression.Insert(0, Convert.ToByte(keys.Count));
                finalCompression.Insert(0, Convert.ToByte(longitudDeBits));
            }
        }
        void FistCompression(List<byte> cadenaByte)
        {
            List<byte> lastCadenaByte = new List<byte>();
            foreach (var item in cadenaByte)
            {
                lastCadenaByte.Add(item);
            }
            if (lastCadenaByte.Count > 1)
            {
                lastCadenaByte.RemoveAt(lastCadenaByte.Count - 1);
                if (ContainsValue(lastCadenaByte, principalCodesData))
                {
                    int key = KeyValue(lastCadenaByte, principalCodesData);
                    firstCompresion.Add(key);
                }
                else if (ContainsValue(lastCadenaByte, secondaryCodesData))
                {
                    int key = KeyValue(lastCadenaByte, secondaryCodesData);
                    firstCompresion.Add(key);
                }
            }
        }

        int KeyValue(List<byte> cadena, Dictionary<int, List<byte>> dictionari)
        {
            var keys = dictionari.Keys.ToList();
            var values = dictionari.Values.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                if (ComparesList(values[i],(cadena)))
                {
                    return keys[i];
                }
            }
            return -1;
        }

        bool ComparesList(List<byte> cadena, List<byte> cadena2)
        {
            if (cadena.Count == cadena2.Count)
            {
                for (int i = 0; i < cadena.Count; i++)
                {
                    if (cadena[i] != cadena2[i]) return false;
                }
            }
            else
            {
                return false;
            }
            return true;
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
            var keys = dictionary.Keys.ToList();
            var auxKeys = principalCodesData.Keys.ToList();

            if(keys.Count>0)
            {
                if (keys.SequenceEqual(auxKeys))
                {
                    for (int i = keys[0]; i <= dictionary.Count(); i++)
                    {
                        var item = dictionary[i];
                        if (ComparesList(item,cadenaByte)) return true;
                    }
                }
                else
                {
                    for (int i = 0; i <= dictionary.Count() - 1; i++)
                    {
                        var item = dictionary[keys[i]];
                        if (ComparesList(item,cadenaByte)) return true;
                    }
                }
            }
            return false;
        }

        public byte[] DecodeData(byte[] content)
        {
            throw new NotImplementedException();
        }

    }
}
