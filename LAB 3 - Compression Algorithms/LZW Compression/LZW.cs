using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Transactions;

namespace LAB_3___Compressor.LZW_Commpression
{
    public class LZW : Interfaces.ICompressionAlgorithm
    {

        Dictionary<int, List<byte>> principalCodesData = new Dictionary<int, List<byte>>();
        Dictionary<int, List<byte>> secondaryCodesData = new Dictionary<int, List<byte>>();


        int count_chars;
        double original_weight;
        double compressed_weight;

        public List<int> firstCompression = new List<int>();

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

        #region Encode 
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
                    principalCodesData.Add( SetKey(0),cadenaByte);
                }
            }
            cadenaByte = new List<byte>();

            for (int i = 0; i < content.Length; i++)
            {
                var item = content[i];
                cadenaByte.Add(item);
                if (!ContainsValue(cadenaByte, principalCodesData) && !ContainsValue(cadenaByte, secondaryCodesData))
                {
                    secondaryCodesData.Add(SetKey(1), cadenaByte);
                    FistCompression(cadenaByte);
                    cadenaByte = new List<byte>();
                    i--;
                }
            }
            if (ContainsValue(cadenaByte,principalCodesData))
                firstCompression.Add(KeyValue(cadenaByte, principalCodesData));
            else
            firstCompression.Add(KeyValue(cadenaByte, secondaryCodesData));

            cadenaByte = new List<byte>();

            original_weight = content.Length;
            return SecondCompression();
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
                    firstCompression.Add(key);
                }
                else if (ContainsValue(lastCadenaByte, secondaryCodesData))
                {
                    int key = KeyValue(lastCadenaByte, secondaryCodesData);
                    firstCompression.Add(key);
                }
            }
        }// agraga valores ala lista de claves
        byte[] SecondCompression()
        {
            List<byte> finalCompression = new List<byte>();
            string binaryCode = "";
            var binaryMax = Convert.ToString(firstCompression.Max(),2).Length;
            int count = 0;
            foreach (var item in firstCompression)
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
            AddMetaData(finalCompression, binaryMax);
            compressed_weight = finalCompression.Count;
            return finalCompression.ToArray();
        } // Comprime la lista de claves 
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
        }//Agrega la metadata al principio la compresion final
        int KeyValue(List<byte> cadena, Dictionary<int, List<byte>> dictionari)
        {
            var keys = dictionari.Keys.ToList();
            var values = dictionari.Values.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                if (CompareList(values[i],(cadena)))
                {
                    return keys[i];
                }
            }
            return -1;
        }//Obtiene la clave de un valor en el diccionario
        bool CompareList(List<byte> cadena, List<byte> cadena2)
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
        } // compara las cadenas si son iguales
        int SetKey(int indicador)
        {
            if (indicador == 1)
                return principalCodesData.Count + secondaryCodesData.Count + 1;
            else if (indicador == 0 ) return principalCodesData.Count + 1;
            else throw new NotImplementedException();
        } //Crea la clave para el diccionario, necesita saber para que diccionario va a generar la clave 
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
                        if (CompareList(item, cadenaByte))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i <= dictionary.Count() - 1; i++)
                    {
                        var item = dictionary[keys[i]];
                        if (CompareList(item,cadenaByte)) return true;
                    }
                }
            }
            return false;
        }//Verifica si el diccionario contiene la lista de bytes
        #endregion
        public byte[] DecodeData(byte[] Content)
        {
            List<byte> content = Content.ToList();
            List<byte> outContent = new List<byte>();
            List<byte> nuevaCadena = new List<byte>();

            var originalBinary = content[0];
            content.RemoveAt(0);

            var tableLongth = content[0];
            content.RemoveAt(0);

            for (int i = 0; i < tableLongth; i++)
            {
                List<byte> temp = new List<byte>();
                temp.Add(content[0]);
                principalCodesData.Add(SetKey(0), temp);
                content.RemoveAt(0);
            }

            AddDescompression(content, originalBinary);

            int keyOld = 0;
            int keyNew = 0;

            List<byte> cadenaSalida = new List<byte>();
            byte newCaracter = 0;

            keyOld = firstCompression[0];
            cadenaSalida.AddRange(GetValueKey(keyOld, principalCodesData));
            newCaracter = cadenaSalida[0];
            outContent.AddRange(cadenaSalida);

            for (int i = 1; i < firstCompression.Count; i++)
            {
                keyNew = firstCompression[i];
                if (!principalCodesData.ContainsKey(keyNew))
                {
                    cadenaSalida.AddRange(GetValueKey(keyOld, principalCodesData));
                    cadenaSalida.Add(newCaracter);
                }
                else
                {
                    cadenaSalida = GetValueKey(keyNew, principalCodesData);
                }

                outContent.AddRange(cadenaSalida);

                nuevaCadena.AddRange(GetValueKey(keyOld, principalCodesData));
                newCaracter = cadenaSalida[0];
                nuevaCadena.Add(newCaracter);
                principalCodesData.Add(SetKey(0), nuevaCadena);
                keyOld = keyNew;

                nuevaCadena = new List<byte>();
                cadenaSalida = new List<byte>();
            }

            return outContent.ToArray();
        }

      
        List<byte> GetValueKey(int key, Dictionary<int, List<byte>> dictionary)
        {
            List<byte> cadena = new List<byte>();

            var keys = dictionary.Keys.ToList();
            var values = dictionary.Values.ToList();

            for (int i = 0; i < values.Count; i++)
            {
                if (keys[i] == key)
                {
                    return values[i];
                }
            }
            return cadena;
        }

        void AddDescompression(List<byte> content, int originalBinary)
        {
            string binaryCode = "";
            int count = 0;
            foreach (var item in content)
            {
                var newBinary = Convert.ToString(item, 2);
                if (newBinary.Length <= 8)
                {
                    count++;
                    //Balance de 0 faltantes para llagar al maximo re querido
                    while (newBinary.Length != 8)
                    {
                        newBinary = "0" + newBinary;
                    }
                    binaryCode += newBinary;
                }
            }
            var splitSize = originalBinary;

            for (int i = 0; i < binaryCode.Length; i += splitSize)
            {
                if (i + splitSize > binaryCode.Length)
                {
                    return;
                }
                else
                {
                    var newInt = Convert.ToInt32(binaryCode.Substring(i, splitSize), 2);
                    firstCompression.Add(newInt);
                }
            }

        }
    }
}
