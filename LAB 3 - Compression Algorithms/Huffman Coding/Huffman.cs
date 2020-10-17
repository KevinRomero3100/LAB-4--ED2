using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LAB_3___Compressor.DataStructures;

namespace LAB_3___Compressor.Huffman_Coding
{
    public class Huffman : Interfaces.ICompressionAlgorithm
    {
        Dictionary<byte, string> codes_encode = new Dictionary<byte, string>();
        Dictionary<string, byte> codes_decode = new Dictionary<string, byte>();

        PriorityQueue<CharacterNode> queue = new PriorityQueue<CharacterNode>();
        List<byte> metadata = new List<byte>();

        int count_chars;
        double original_weight;
        double compressed_weight;

        public byte[] EncodeData(byte[] content)
        {
            count_chars = content.Length;
            queue.PriorityComparison = PriorityComparison;

            //count all frequencies
            List<CharacterNode> base_frequencies = ProbeFrequencies(content);

            CharacterNode root = CreateTree(base_frequencies);
            GenerateCodes(root, "");

            //swap each char for its binary code
            string content_encoded = "";
            for (int i = 0; i < content.Length; i++)
            {
                content_encoded += codes_encode[content[i]];
            }

            //complete the last octet
            while (content_encoded.Length % 8 != 0)
            {
                content_encoded += "0";
            }

            //converting each binary code to a decimal number and then converting to byte
            List<byte> encode_final = metadata;
            for (int i = 0; i < content_encoded.Length; i += 8)
            {
                string code_bin = content_encoded.Substring(i, 8);
                int code_dec = Convert.ToInt32(code_bin, 2);
                encode_final.Add(Convert.ToByte(code_dec));
            }

            original_weight = content.Length;
            compressed_weight = encode_final.Count;
             
            return encode_final.ToArray();
        }


        private CharacterNode CreateTree(List<CharacterNode> list)
        {

            List<CharacterNode> list_complete = SetPercentages(list);

            SetMetaData(list_complete);
            SetQueue(list_complete);


            while (queue.Count != 1)
            {
                CharacterNode left_node = queue.Pop();
                CharacterNode right_node = queue.Pop();

                CharacterNode intermadiate_note = new CharacterNode() { Percentage= (left_node.Percentage + right_node.Percentage) ,Left = left_node, Right = right_node};
                queue.Push(intermadiate_note);
            }
            return queue.Pop();
        }


        private void GenerateCodes(CharacterNode root, string current_code)
        {
            if (root.Left == null && root.Right == null)
            {
                root.Code = current_code;
                codes_encode.Add(root.Character, root.Code);
                codes_decode.Add(root.Code, root.Character);
                return;
            }
            if (root.Left != null)
            {
                current_code += "0";
                GenerateCodes(root.Left, current_code);
                current_code = current_code.Substring(0, current_code.Length - 1);
            }
            if (root.Right != null)
            {
                current_code += "1";
                GenerateCodes(root.Right, current_code);
            }
        }

        private List<CharacterNode> ProbeFrequencies(byte[] text)
        {
            List<CharacterNode> data_list = new List<CharacterNode>();
            int text_length = text.Length;
            for (int i = 0; i < text_length; i++)
            {
                int position_character = DetectPositionCharacter(data_list, text[i]);
                if (position_character != -1)
                {
                    data_list.ElementAt(position_character).Frequency++;
                }
                else
                {
                    CharacterNode new_character = new CharacterNode { Character = text[i], Frequency = 1 };
                    data_list.Add(new_character);
                }
            }
            return data_list;
        }

        private List<CharacterNode> SetPercentages(List<CharacterNode> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                double frequency = Convert.ToDouble(list[i].Frequency);
                double total = Convert.ToDouble(count_chars);
                double percentage = frequency / total;

                list[i].Percentage = percentage;
            }
            return list;
        }

        private void SetMetaData(List<CharacterNode> list)
        {
            //number of different characters
            metadata.Add(Convert.ToByte(list.Count));

            //PRUEBA////////////////////////////////////////////////////////
            int max_frequency = FindMaxLenght(list);
            string bin = Convert.ToString(max_frequency, 2);

            ///normalize the max lenght
            while (bin.Length % 8 != 0)
            {
                bin = "0" + bin;
            }

            //number of bytes for each character frequencie
            int number_bytes = bin.Length / 8;
            metadata.Add(Convert.ToByte(number_bytes));


            for (int i = 0; i < list.Count; i++)
            {
                CharacterNode _node = list[i];
                metadata.Add(Convert.ToByte(_node.Character));

                byte[] complete_frequency = GetNormalizeFrequency(_node.Frequency, number_bytes);
                for (int j = 0; j < complete_frequency.Length; j++)
                {
                    metadata.Add(complete_frequency[j]);
                }
            }
        }

        private int FindMaxLenght(List<CharacterNode> list)
        {
            List<CharacterNode> order_list = list.OrderByDescending(x => x.Frequency).ToList();
            return order_list[0].Frequency;
        }

        private byte[] GetNormalizeFrequency(int frequency, int bytes_count)
        {
            List<byte> c_frequency = new List<byte>(bytes_count);

            //if the nortmalize is innecesary
            if (frequency < 256)
            {
                for (int i = 0; i < bytes_count-1; i++)
                {
                    c_frequency.Add(0);
                }
                c_frequency.Add(Convert.ToByte(frequency));
                return c_frequency.ToArray();
            }

            ///normalize the current frequency
            string bin = Convert.ToString(frequency, 2);
            while (bin.Length % 8 != 0)
            {
                bin = "0"+bin;
            }

            for (int i = 0; i < bin.Length; i += 8)
            {
                string split_bit = bin.Substring(i, 8);
                int byte_converted = Convert.ToInt32(split_bit, 2);
                c_frequency.Add(Convert.ToByte(byte_converted));
            }

            return c_frequency.ToArray();
        }

        private int DetectPositionCharacter(List<CharacterNode> data_list, byte character)
        {
            for (int i = 0; i < data_list.Count; i++)
            {
                if (data_list.ElementAt(i).Character == character)
                {
                    return i;
                }
            }
            return -1;
        }

        private void SetQueue(List<CharacterNode> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                queue.Push(list[i]);
            }
        }

        public byte[] DecodeData(byte[] content)
        {
            int characters = content[0];
            int length = content[1];

            //generates the codes with the metadata
            queue.PriorityComparison = PriorityComparison;
            List<CharacterNode> list = new List<CharacterNode>(characters);

            if (length == 2)
            {
                int chars_added = 0;
                int index = 2;
                while (chars_added < characters)
                {
                    byte char_byte = content[index];

                    List<byte> complex_frequency = new List<byte>();
                    complex_frequency.Add(content[index + 1]);
                    complex_frequency.Add(content[index + 2]);
                    int total_frequency = GetFrequency(complex_frequency);


                    CharacterNode c_node = new CharacterNode() { Character = char_byte, Frequency = total_frequency };
                    list.Add(c_node);
                    count_chars += total_frequency;

                    index += length + 1;
                    chars_added++;
                }
                CharacterNode root = CreateTree(list);
                GenerateCodes(root, "");
            }
            if (length == 1)
            {
                int chars_added = 0;
                int index = 2;
                while (chars_added < characters)
                {
                    byte char_byte = content[index];
                    int total_frequency = content[index + 1];


                    CharacterNode c_node = new CharacterNode() { Character = char_byte, Frequency = total_frequency };
                    list.Add(c_node);
                    count_chars += total_frequency;

                    index += length + 1;
                    chars_added++;
                }
                CharacterNode root = CreateTree(list);
                GenerateCodes(root, "");
            }

            //convert the bytes to binary code
            string content_decoded = "";
            int start = ((length + 1) * characters) + 2;
            for (int i = start; i < content.Length; i++)
            {
                int dec = content[i];
                string bin = Convert.ToInt32(Convert.ToString(dec, 2)).ToString("D8");
                content_decoded += bin;
            }

            //replace all binary code to the equivalent byte
            List<byte> result = new List<byte>();
            string current_code = "";

            int replaced = 0;
            int j = 0;
            while (replaced<count_chars)
            {
                current_code += content_decoded[j];
                if (codes_decode.ContainsKey(current_code))
                {
                    result.Add(codes_decode[current_code]);
                    current_code = "";
                    replaced++;
                }
                j++;
            }

            return result.ToArray();
        }

        private int GetFrequency(List<byte> values)
        {
            string partial_binary = "";
            for (int i = 0; i < values.Count; i++)
            {
                int dec = values[i];
                partial_binary = partial_binary + Convert.ToInt32(Convert.ToString(dec, 2)).ToString("D8");

            }
            int final_desc = Convert.ToInt32(partial_binary, 2);
            return final_desc;
        }
        
        public double ReductionPercentage()
        {
            return 1 - (compressed_weight/original_weight);
        }

        public double CompressionFactor()
        {
            return (original_weight / compressed_weight);
        }

        public double CompressionRatio()
        {
            return (compressed_weight / original_weight);
        }

        private static Comparison<CharacterNode> PriorityComparison = delegate (CharacterNode node1, CharacterNode node2)
        {
            if (node1.Percentage > node2.Percentage) return 1;
            if (node1.Percentage < node2.Percentage) return -1;
            return 0;

        };



    }
}
