using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Microsoft.Office.Interop.Word;

namespace encryption.model.Steganography
{
    public enum Encoding1 { Unicode, win1251, cp866, koi8u };

    public static class Steganography
    {
        private static Random rnd = new Random();



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
            doc.Content.Font.Size = 14;
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
            switch(enc)
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

    }
}
