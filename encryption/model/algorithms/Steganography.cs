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
//using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;

namespace encryption.model.Steganography
{
    public enum Encoding1 { Unicode, win1251, cp866, koi8u };

    public static class Steganography
    {
        private static Random rnd = new Random();

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
