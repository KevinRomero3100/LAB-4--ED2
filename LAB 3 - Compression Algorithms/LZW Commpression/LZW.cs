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
        List<string> DecodeDictionary = new List<string>();
        List<List<byte>> Decode = new List<List<byte>>();


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
                firstCompresion.Add(KeyValue(cadenaByte, principalCodesData));
            else
            firstCompresion.Add(KeyValue(cadenaByte, secondaryCodesData));

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
                    firstCompresion.Add(key);
                }
                else if (ContainsValue(lastCadenaByte, secondaryCodesData))
                {
                    int key = KeyValue(lastCadenaByte, secondaryCodesData);
                    firstCompresion.Add(key);
                }
            }
        }// agraga valores ala lista de claves
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
                        if (CompareList(item,cadenaByte)) return true;
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
        public byte[] DecodeData(byte[] content)
        {
            int bits_lenght = content[0];
            int table_lenght = content[1];


            int count = 0;
            int pivote = 0;
            DecodeDictionary.Add(null);
            //
            Decode.Add(null);
            //
            while (count < table_lenght)
            {
                DecodeDictionary.Add(Convert.ToString((char)content[pivote + 2]));

                //
                List<byte> current_char = new List<byte> { content[pivote + 2] }; 
                Decode.Add(current_char);
                //
                pivote++;
                count++;
            }

            string text = "";
            int start = table_lenght + 2;
            while (start < content.Length)
            {
                int dec = (int)content[start];
                string bin = Convert.ToInt32(Convert.ToString(dec, 2)).ToString("D8");

                text += bin;
                start++;
            }

            List<int> text_decoded = new List<int>();
            string current_index = "";
            for (int i = 0; i < text.Length; i++)
            {
                current_index += text[i];
                if (current_index.Length % bits_lenght == 0)
                {
                    int dec = Convert.ToInt32(current_index, 2);
                    if (dec == 0) break;
                    text_decoded.Add(dec);
                    current_index = "";
                }
            }

            //string result = "";
            List<byte> result2 = new List<byte>();

            //string last_chain = "";
            List<byte> last_chain2 = new List<byte>();

            int test = 0;
            try
            {
                for (int i = 0; i < text_decoded.Count; i++)
                {
                    ///
                    //string current_chain = DecodeDictionary[text_decoded[i]];
                    List<byte> current_chain2 = new List<byte>();
                    CopyChain(current_chain2,Decode[text_decoded[i]]);

                    //
                   // result += current_chain;
                    result2 = result2.Concat(current_chain2).ToList();
                    ///


                    // string complete_chain = last_chain + current_chain[0];
                    byte temporal_byte = current_chain2[0];
                    last_chain2.Add(temporal_byte);
                    List<byte> complete_chain2 = last_chain2;

                    if (test == 47)
                    {
                        
                    }

                    if (!ContainsChain(complete_chain2))
                    {
                       // DecodeDictionary.Add(complete_chain);
                        Decode.Add(complete_chain2);
                    } 
                    //last_chain = current_chain;
                    last_chain2 = current_chain2;
                    test++;
                }
            }
            catch (Exception)
            {

                int stop = test;
            }


            //List<byte> result_normalize = ConvertText(result);
            //return result_normalize.ToArray();
            return result2.ToArray();
        }

        public bool ContainsChain(List<byte> chain)
        {
            for (int i = 1; i < Decode.Count; i++)
            {
                List<byte> current_chain = Decode[i];
                if (CompareChain(current_chain, chain)) return true;
            }
            return false;
        }

        public bool CompareChain(List<byte> chain1, List<byte> chain2)
        {
            if (chain1.Count != chain2.Count) return false;
            for (int i = 0; i < chain1.Count; i++)
            {
                if (chain1[i] != chain2[i]) return false;
            }
            return true;
        }

        public void CopyChain(List<byte> duplicate, List<byte> original)
        {
            for (int i = 0; i < original.Count; i++)
            {
                duplicate.Add(original[i]);
            }
        }

        public List<byte> ConvertText(string result)
        {
            List<byte> result_normalize = new List<byte>();
            for (int i = 0; i < result.Length; i++)
            {
                result_normalize.Add((byte)result[i]);
            }
            return result_normalize;
        }
    }
}
