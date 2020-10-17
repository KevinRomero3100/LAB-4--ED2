using System;
using System.Collections.Generic;
using System.Text;

namespace LAB_3___Compressor.Huffman_Coding
{
    public class CharacterNode
    {
        public byte Character { get; set; }
        public int Frequency { get; set; }
        public double Percentage { get; set; }
        public string Code { get; set; }
        public CharacterNode Left { get; set; }
        public CharacterNode Right { get; set; }

        public CharacterNode()
        {
            Character = 00;
            Frequency = 0;
            Percentage = 0;
            Code = " ";
            Left = null;
            Right = null;
        }

    }
}
