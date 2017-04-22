using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Microsoft.Office.Interop.Word;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.IO.Compression;

namespace encryption.model.Steganography
{
    public enum Encoding1 { Unicode, win1251, cp866, koi8u };

    public static class Steganography
    {
        private static Random rnd = new Random();

        //извлекает байтовый массив из bmp24
        public static byte[] TakesBytesFromBMP(string path)
        {
            Bitmap bmp = new Bitmap(path);
            int width = bmp.Width;
            int height = bmp.Height;
            bmp.Dispose();

            int space = width % 4;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(10, SeekOrigin.Begin);
            byte[] bytes = new byte[4];
            fs.Read(bytes, 0, 4);
            BigInteger big = new BigInteger(bytes);
            fs.Seek((long)big, SeekOrigin.Begin);
            byte[] buff = new byte[3 * width];

            uint symbCount = 0;
            for (int i = 0; i < 16; i++)
            {
                byte bbb = (byte)fs.ReadByte();
                bbb <<= 6;
                bbb >>= 6;
                symbCount |= bbb;
                if (i != 15) symbCount <<= 2;
            }

            fs.Seek((long)(big + 16), SeekOrigin.Begin);     //начало считывания символов
            byte[] symbs = new byte[symbCount];
            int K = 1;
            int nn = 0;
            int ll = 0;
            while (width >= K)
            {
                if (K == 1) fs.Read(buff, 0, 3 * width - 16);
                else fs.Read(buff, 0, 3 * width);
                for (int i = 0; i < buff.Length - (K == 1 ? 16 : 0); i++)
                {
                    buff[i] <<= 6;
                    buff[i] >>= 6;
                    switch (ll)
                    {
                        case 0:
                            symbs[nn] |= buff[i];
                            symbs[nn] <<= 2;
                            ll++;
                            break;
                        case 1:
                            symbs[nn] |= buff[i];
                            symbs[nn] <<= 2;
                            ll++;
                            break;
                        case 2:
                            symbs[nn] |= buff[i];
                            symbs[nn] <<= 2;
                            ll++;
                            break;
                        case 3:
                            symbs[nn] |= buff[i];
                            ll = 0;
                            nn++;
                            if (nn >= symbs.Length)
                                goto dads;
                            break;
                    }
                }
                fs.Seek(space, SeekOrigin.Current);
                K++;
            }
        dads:;


            fs.Close();
            return symbs;

        }

        //прячет поток байтов в графический файл bmp24
        public static void HideBytesInBMP(byte[] bytes, string path)
        {
            Bitmap bmp = new Bitmap(path);
            int width = bmp.Width;
            int height = bmp.Height;
            bmp.Dispose();

            int space = width % 4;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(10, SeekOrigin.Begin);
            byte[] bytesn = new byte[4]; 
            fs.Read(bytesn, 0, 4);
            BigInteger big = new BigInteger(bytesn);
            fs.Seek((long)big, SeekOrigin.Begin);
            byte[] buff = new byte[3 * width];

            Console.WriteLine("кол-во байт возм.: {0}", width * height * 3 / 4);
            Console.WriteLine("bytes: {0}", bytes.Length + 2);

            //формирование байтового массива
            byte[] symbs = bytes;
            byte[] symbsTemp = new byte[symbs.Length + 4];
            uint symbCount = (uint)symbs.Length;
            symbsTemp[0] |= (byte)(symbCount >> 24);
            symbsTemp[1] |= (byte)(symbCount >> 16);
            symbsTemp[2] |= (byte)(symbCount >> 8);
            symbsTemp[3] |= (byte)symbCount;
            for (int i = 4; i < symbsTemp.Length; i++)
                symbsTemp[i] = symbs[i - 4];
            symbs = symbsTemp;

            //запись битов
            int K = 1;
            int nn = 0;
            int ll = 0;
            StringBuilder strRes = new StringBuilder();
            while (width >= K)
            {
                fs.Read(buff, 0, 3 * width);

                for (int i = 0; i < buff.Length; i++)
                {
                    buff[i] >>= 2;
                    buff[i] <<= 2;
                    byte bbb;
                    switch (ll)
                    {
                        case 0:
                            buff[i] |= (byte)(symbs[nn] >> 6);
                            ll++;
                            break;
                        case 1:
                            bbb = (byte)(symbs[nn] << 2);
                            buff[i] |= (byte)(bbb >> 6);
                            ll++;
                            break;
                        case 2:
                            bbb = (byte)(symbs[nn] << 4);
                            buff[i] |= (byte)(bbb >> 6);
                            ll++;
                            break;
                        case 3:
                            bbb = (byte)(symbs[nn] << 6);
                            buff[i] |= (byte)(bbb >> 6);
                            ll = 0;
                            nn++;
                            if (nn >= symbs.Length)
                            {
                                fs.Seek(-3 * width, SeekOrigin.Current);
                                fs.Write(buff, 0, 3 * width);
                                goto dads;
                            }
                            break;
                    }
                }
                fs.Seek(-3 * width, SeekOrigin.Current);
                fs.Write(buff, 0, 3 * width);
                fs.Seek(space, SeekOrigin.Current);
                K++;
            }
        dads:;

            fs.Close();
        }

        //распаковывает поток байтов
        public static byte[] UnZipBytes(byte[] gzip)
        {
            byte[] res;
            using (MemoryStream originalFileStream = new  MemoryStream(gzip))
            {
                using (MemoryStream decompressedFileStream = new MemoryStream())
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                    res = decompressedFileStream.ToArray();
                }
            }

            return res;
        }

        //упаковывает поток байтов
        public static byte[] ZipBytes(byte[] bs)
        {
            byte[] res;
            using (MemoryStream sourceStream = new MemoryStream(bs))
            {
                using (MemoryStream targetStream = new MemoryStream())
                {
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream);
                        
                    }
                    res = targetStream.ToArray();
                }
            }

            return res;
        }

        //извлекает байтовый массив из docx файла
        public static byte[] TakeBytesFromDoc(string path, Encoding1 enc = Encoding1.Unicode)
        {
            Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
            object filename = path;
            Document doc = app.Documents.Open(ref filename);

            StringBuilder strBits = new StringBuilder();
            for (int i = 0; i < doc.Characters.Count; i++)
            {
                object start = i;
                object end = i + 1;
                bool bit = false;
                Range range = doc.Range(ref start, ref end);
                switch (enc)
                {
                    case Encoding1.cp866:
                        if (range.Font.Color == (WdColor)ColorTranslator.ToOle(Color.FromArgb(0, 0, 0, 1)))
                            bit = true;
                        break;
                    case Encoding1.win1251:
                        if (range.Font.Size == (float)14.5) bit = true;
                        break;
                    case Encoding1.koi8u:
                        if (range.Font.Scaling == 101) bit = true;
                        break;
                    case Encoding1.Unicode:
                        if (range.Font.Spacing == (float)0.1) bit = true;
                        break;
                }
                if (bit) strBits.Append('1');
                else strBits.Append('0');
            }

            List<string> bytes = new List<string>();

            int kk = 0;
            int k0 = 0;
            StringBuilder strB = new StringBuilder();
            for (int i = 0; i < strBits.Length; i++)
            {
                strB.Append(strBits[i]);
                kk++;
                if (kk % 8 == 0)
                {
                    bytes.Add(strB.ToString());
                    if (strB.ToString() == "00000000") k0++;
                    else k0 = 0;
                    strB = new StringBuilder();
                }
                if (k0 == 3)
                {
                    if (enc == Encoding1.Unicode) bytes.RemoveRange(bytes.Count - 2, 2);
                    else bytes.RemoveRange(bytes.Count - 3, 3);
                    break;
                }
            }

            byte[] byts = new byte[bytes.Count];
            for (int i = 0; i < bytes.Count; i++)
                byts[i] = GetByteFromBitString(bytes[i]);

            doc.Close();
            app.Quit();

            return byts;
        }

        //прячет байтовый массив в docx файл
        public static void HideBytesInDoc(byte[] b, string path, Encoding1 enc = Encoding1.Unicode)
        {
            Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
            object filename = path;
            Document doc = app.Documents.Open(ref filename);

            StringBuilder strOfBits = new StringBuilder();
            foreach (byte item in b)
                strOfBits.Append(GetBits(item));

            for (int i = 0; i < strOfBits.Length; i++)
                if (strOfBits[i] == '1')
                    switch (enc)
                    {
                        case Encoding1.cp866:
                            doc.Characters[i + 1].Font.Color = (WdColor)ColorTranslator.ToOle(Color.FromArgb(0, 0, 0, 1));
                            break;
                        case Encoding1.win1251:
                            doc.Characters[i + 1].Font.Size = (float)14.5;
                            break;
                        case Encoding1.koi8u:
                            doc.Characters[i + 1].Font.Scaling = 101;
                            break;
                        case Encoding1.Unicode:
                            doc.Characters[i + 1].Font.Spacing = (float)0.1;
                            break;
                    }

            doc.Save();
            doc.Close();
            app.Quit();
        }

        //получает содержание файла docx
        public static string GetDocText(string pathOfFile)
        {
            Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
            object filename = pathOfFile;
            Document doc = app.Documents.Open(ref filename);
            string text = doc.Content.Text;
            doc.Close();
            app.Quit();

            return text;
        }

        //создает документ и записывает в него текст
        public static void CreateDoc(string text, string pathOfFile)
        {
            Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
            Document doc = app.Documents.Add();
            doc.Content.Text = text;
            doc.Content.Font.Name = "Consolas";
            doc.Content.Font.Color = (WdColor)ColorTranslator.ToOle(Color.FromArgb(0, 0, 0, 0));
            doc.Content.Font.Size = (float)14;
            doc.Content.Font.Scaling = 100;
            doc.Content.Font.Spacing = 0;
            object path = pathOfFile;
            doc.SaveAs(ref path);
            doc.Close();
            app.Quit();
        }

        //получение строки из битового массива
        public static string GetString(byte[] source, Encoding1 enc = Encoding1.Unicode)
        {
            Encoding encoding = Encoding.Unicode;
            switch (enc)
            {
                case Encoding1.win1251:
                    encoding = Encoding.GetEncoding("windows-1251");
                    break;
                case Encoding1.cp866:
                    encoding = Encoding.GetEncoding("cp866");
                    break;
                case Encoding1.koi8u:
                    encoding = Encoding.GetEncoding("koi8-u");
                    break;
            }

            string str = encoding.GetString(source);
            return str;
        }

        //запись строки в битовый массив
        public static byte[] GetBytes(string source, Encoding1 enc = Encoding1.Unicode)
        {
            Encoding encoding = Encoding.Unicode;
            switch (enc)
            {
                case Encoding1.win1251:
                    encoding = Encoding.GetEncoding("windows-1251");
                    break;
                case Encoding1.cp866:
                    encoding = Encoding.GetEncoding("cp866");
                    break;
                case Encoding1.koi8u:
                    encoding = Encoding.GetEncoding("koi8-u");
                    break;
            }

            byte[] bb = encoding.GetBytes(source);

            return bb;
        }

        //получить биты
        public static string GetBits(byte b)
        {
            StringBuilder res = new StringBuilder();
            for (int i = 7; i >= 0; i--)
            {
                byte jj = 1;
                jj <<= i;
                jj &= b;
                jj >>= i;
                res.Append(jj.ToString());
            }

            return res.ToString();
        }

        //получить байт
        public static byte GetByteFromBitString(string bits)
        {
            byte res = 0;

            for (int i = 0; i < 8; i++)
            {
                res |= (byte)(bits[i] == '1' ? 1 : 0);
                if (i != 7) res <<= 1;
            }

            return res;
        }


    }
}
