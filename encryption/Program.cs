using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using encryption.model.SymmetricEncryption;
using encryption.model.AsymmetricEncryption;
using System.Numerics;

namespace encryption
{
    class Program
    {
        const sbyte KEY1 = 4;
        const string KEY2 = "криптография";
        const int KEY3 = 3;
        const string KEY4 = "шифр";
        const string KEY5 = "CIPHERTEXT";
        const string KEY6 = "LEMON";
        const string SOURCE1 = "перваялабораторнаяработапокиоки";
        const string SOURCE2 = "example";
        const string SOURCE3 = "договорподписали";
        const string SOURCE4 = "CRYPTOGRAPHY";
        const string SOURCE5 = "VIGENERE";//"ATTACKATDAWN";

        static void Main(string[] args)
        {
            string res;/*
            #region Метод "Железнодорожной изгороди"
            Console.WriteLine("Метод \"Железнодорожной изгороди\":");
            Console.WriteLine("Первичная информация: {0}", SOURCE1);
            res = EncryptSymmetric.ZigZagCipher(SOURCE1, KEY1);
            Console.WriteLine("После шифрования:     {0}", res);
            res = EncryptSymmetric.ZigZagDeCipher(res, KEY1);
            Console.WriteLine("После дешифровки:     {0}\n", res);
            #endregion

            #region "Столбцовый метод"
            Console.WriteLine("\"Столбцовый\" метод:");
            Console.WriteLine("Первичная информация: {0}", SOURCE1);
            res = EncryptSymmetric.ColumnCipher(SOURCE1, KEY2);
            Console.WriteLine("После шифрования:     {0}", res);
            res = EncryptSymmetric.ColumnDeCipher(res, KEY2);
            Console.WriteLine("После дешифровки:     {0}\n", res);
            #endregion

            #region Шифр Цезаря
            Console.WriteLine("Шифр Цезаря:");
            Console.WriteLine("Первичная информация: {0}", SOURCE2);
            res = EncryptSymmetric.CaesarCipher(SOURCE2, KEY3, AlphabetForEncryption.Latin);
            Console.WriteLine("После шифрования:     {0}", res);
            res = EncryptSymmetric.CaesarDeCipher(res, KEY3, AlphabetForEncryption.Latin);
            Console.WriteLine("После дешифровки:     {0}\n", res);
            #endregion

            #region Метод "поворотной решетки"
            Console.WriteLine("Метод \"Поворотной решетки\":");
            Console.WriteLine("Первичная информация: {0}", SOURCE3);
            byte[,] grid = { { 0, 0, 0, 1 },
                             { 1, 0, 0, 0 },
                             { 0, 0, 1, 0 },
                             { 0, 0, 1, 0 } };
            res = EncryptSymmetric.TurnedGridCipher(SOURCE3, grid, KEY4);
            Console.WriteLine("После шифрования:     {0}", res);
            res = EncryptSymmetric.TurnedGridDeCipher(res, grid, KEY4);
            Console.WriteLine("После дешифровки:     {0}\n", res);
            #endregion

            Console.WriteLine("Шифр Плейфера:");
            Console.WriteLine("Первичная информация: {0}", SOURCE4);
            res = EncryptSymmetric.PlayfairCipher(SOURCE4, KEY5);
            Console.WriteLine("После шифрования:     {0}", res);
            res = EncryptSymmetric.PlayfairDeCipher(res, KEY5);
            Console.WriteLine("После дешифровки:     {0}\n", res);

            Console.WriteLine("Шифр Виженера:");
            Console.WriteLine("Первичная информация: {0}", SOURCE5);
            res = EncryptSymmetric.VigenereCipher(SOURCE5, KEY6);
            Console.WriteLine("После шифрования:     {0}", res);
            res = EncryptSymmetric.VigenereDeCipher(res, KEY6);
            Console.WriteLine("После дешифровки:     {0}\n", res);
            */
            DateTime time = DateTime.Now;
            var rsaKeys = AsymmetricEncryption.GetRSAKeys(KeyAmount.b256);
            TimeSpan per = DateTime.Now - time;
            Console.WriteLine("Estimate time(generate keys): {0}", per);
            Console.WriteLine("Открытый ключ: ({0}, {1})\nЗакрытый ключ: ({2}, {3})", rsaKeys.Item1.Item1, rsaKeys.Item1.Item2,
                rsaKeys.Item2.Item1, rsaKeys.Item2.Item2);

            string str = "My string for Dogs";
            for (int i = 0; i < 5; i++)
                str += str;
            time = DateTime.Now;
            BigInteger[] sss = AsymmetricEncryption.RSAEncrypt(str, rsaKeys.Item1);
            per = DateTime.Now - time;
            Console.WriteLine("Estimate time(encrypt): {0}", per);

            time = DateTime.Now;
            string sstr = AsymmetricEncryption.RSADecrypt(sss, rsaKeys.Item2);
            per = DateTime.Now - time;
            Console.WriteLine("Estimate time(decrypt): {0}", per);
            Console.WriteLine(sstr);
            Console.WriteLine(sstr.Length);

            Console.ReadKey();
        }
        
    }
}
