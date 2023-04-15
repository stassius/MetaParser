using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace MetaParserForms
{
    static class Parser
    {
        static public Dictionary<string, string> ParseBitMap(Stream image)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            var hash = GetMetaData(image);

            if (hash.ContainsKey("parameters") == false) return result;
            string tag = hash["parameters"].ToString();

            string prompt, negPrompt;
            if (tag.Contains("Negative prompt:"))
            {
                prompt = tag.Substring(0, tag.IndexOf("Negative prompt:")).Trim();
                negPrompt = GetInbetweenString(tag, "Negative prompt:", "Steps:").Trim();
            }
            else
            {
                prompt = tag.Substring(0, tag.IndexOf("Steps:")).Trim();
                negPrompt = " ";
            }
            if (string.IsNullOrWhiteSpace(prompt) == false || string.IsNullOrWhiteSpace(negPrompt) == false)
            {
                result.Add("Prompt", prompt);
                result.Add("Negative prompt", negPrompt);
            }

            string rest = tag.Substring(tag.IndexOf("Steps:"));
            foreach (string unit in rest.Split(new Char[] { ',', '\n' }))
            {
                var part = unit.Split(":");
                if (part.Length < 2) continue;
                var argName =part[0].Trim();
                var value = part[1].Trim();
                if (argName == "Size")
                {
                    result.Add("Width", value.Split("x")[0]);
                    result.Add("Height", value.Split("x")[1]);
                }
                else
                {
                    result.Add(argName, value);
                }
            }
            return result;
        }
        private static string GetInbetweenString(string str, string first, string last)
        {
            int pFrom = str.IndexOf(first) + first.Length;
            int pTo = str.LastIndexOf(last);
            if (pTo - pFrom <= 0) return string.Empty;
            return str.Substring(pFrom, pTo - pFrom);
        }

        public static Stream ToStream(this Image image)
        {
            var stream = new MemoryStream();
            image.Save(stream, image.RawFormat);
            stream.Position = 0;
            return stream;
        }


        public static Hashtable GetMetaData(Stream image)
        {
            using (image)
            {
                Hashtable metadata = new Hashtable();
                byte[] imageBytes;

                using (var memoryStream = new MemoryStream())
                {
                    image.CopyTo(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                if (imageBytes.Length <= 8)
                {
                    return null;
                }

                // Skipping 8 bytes of PNG header
                int pointer = 8;

                while (pointer < imageBytes.Length)
                {
                    // read the next chunk
                    uint chunkSize = GetChunkSize(imageBytes, pointer);
                    pointer += 4;
                    string chunkName = GetChunkName(imageBytes, pointer);
                    pointer += 4;

                    // chunk data -----
                    if (chunkName.Equals("tEXt") || chunkName.Equals("iTXt"))
                    {
                        byte[] data = new byte[chunkSize];
                        Array.Copy(imageBytes, pointer, data, 0, chunkSize);
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (byte t in data)
                        {
                            stringBuilder.Append((char)t);
                        }

                        string[] pair;
                        if (chunkName.Equals("iTXt"))
                        {
                            pair = stringBuilder.ToString().Split("\0\0\0\0\0");
                            byte[] bytes = Encoding.Latin1.GetBytes(pair[1]);
                            pair[1] = Encoding.UTF8.GetString(bytes);
                        }
                        else
                        {
                            pair = stringBuilder.ToString().Split(new char[] { '\0' });
                        }
                        metadata[pair[0]] = pair[1];
                        Console.WriteLine(metadata[pair[0]]);
                    }

                    pointer += (int)chunkSize + 4;

                    if (pointer > imageBytes.Length)
                        break;
                }
                return metadata;
            }
        }

        private static uint GetChunkSize(byte[] bytes, int pos)
        {
            byte[] quad = new byte[4];
            for (int i = 0; i < 4; i++) { quad[3 - i] = bytes[pos + i]; }

            return BitConverter.ToUInt32(quad, 0);

        }

        private static string GetChunkName(byte[] bytes, int pos)
        {
            StringBuilder builder = new StringBuilder(); for (int i = 0; i < 4; i++) { builder.Append((char)bytes[pos + i]); }

            return builder.ToString();

        }
        
    }
}
