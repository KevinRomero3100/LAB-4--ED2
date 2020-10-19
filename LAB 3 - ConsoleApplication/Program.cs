﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LAB_3___Compressor.LZW_Commpression;

namespace LAB_3___ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("\t\t\t\t\t\t\t- LAB 3 -\n\nKevin Romero 1047519\nJosé De León 1072619");
            LZW lzw = new LZW();

            string original = "A Cuesta le cuesta subir la cuesta,\ny en medio de la cuesta va y se acuesta";
            Console.WriteLine("\n\nTEXTO ORIGINAL          \n" + original);

            byte[] compression_result  = lzw.EncodeData(ConvertToByte(original));
            Console.WriteLine("\n\nTEXTO COMPRIMIDO");
            Console.WriteLine(ConvertToChar(compression_result));
            
            double compression_factor = lzw.CompressionFactor();
            double compression_ratio = lzw.CompressionRatio();
            double reduction_percentage = lzw.ReductionPercentage();
            

            //lzw = new LZW();
            //byte[] descompression_result = lzw.DecodeData(compression_result);
            //Console.WriteLine("\n\nTEXTO DESCOMPRIMIDO");
            //Console.WriteLine(ConvertToChar(descompression_result));
            
            Console.WriteLine("\n\n\t\tFACTOR DE COMPRESIÓN\t\tRAZÓN DE COMPRESIÓN\t\tPORCENTAJE DE REDUCCIÓN");
            Console.WriteLine("\t\t"+compression_factor +"\t\t"+ compression_ratio + "\t\t"+ reduction_percentage);
            

            Console.ReadLine();
        }

        public static byte[] ConvertToByte(string content)
        {
            byte[] array = new byte[content.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Convert.ToByte(content[i]);
            }
            return array;
        }

        public static char[] ConvertToChar(byte[] content)
        {
            char[] array = new char[content.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Convert.ToChar(content[i]);
            }
            return array;
        }
    }
}
