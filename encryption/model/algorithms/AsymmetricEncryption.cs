using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace encryption.model.AsymmetricEncryption
{
    public enum KeyAmount { b256 = 256, b512 = 512, b1024 = 1024, b2048 = 2048, b4096 = 4096 };

    public static class AsymmetricEncryption
    {
        //генерация большого числа, размеров определенного количества бит
        public static BigInteger GetBigNumber(KeyAmount keyAmount)
        {
            int nBits = (int)keyAmount;
            byte[] bytes = new byte[nBits / 8];
            new Random().NextBytes(bytes);
            return BigInteger.Abs(new BigInteger(bytes));
        }

        //Решето Эратосфена
        public static int[] getSimplicityNumbers(int n)
        {
            //массив чисел от 0 до n включительно
            int[] numbers = new int[n + 1];
            for (int i = 0; i < numbers.Length; i++)
                numbers[i] = i;
            numbers[0] = numbers[1] = -1;

            long p = 2;     //первоначальное число
            do
            {
                long k = p;
                //вычеркиваем неподходящие числа
                for (long j = p * p; j <= n; j += p * (p == 2 ? 1 : 2))
                    numbers[j] = -1;
                //выбираем следующее опорное число
                for (long j = p + 1; j <= n; j++)
                    if (numbers[j] != -1)
                    {
                        p = j;
                        break;
                    }
                if (k == p) break;
            } while (true);
            //формируем массив простых чисел
            int k1 = 0;
            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] != -1) k1++;

            int[] result = new int[k1];

            k1 = 0;
            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] != -1) result[k1++] = numbers[i];

            return result;
        }
    }

    //расширения стандартных классов
    public static class Extensions
    {
        //генерация случайного числа типа BigInteger
        //число выбирается из диапазона [0; bigN)
        public static BigInteger NextBigInteger(this Random rnd, BigInteger bigN)
        {
            for (;;)
            {
                //генерируем новое число размерностью bigN
                byte[] bytes = bigN.ToByteArray();
                rnd.NextBytes(bytes);
                int bitsToRemove = rnd.Next(bytes.Length * 8);      //количество обнуляемых бит
                int kk = bytes.Length - 1;
                for (int i = 0; i < bitsToRemove; i += 8)
                {
                    //обнуляем целый байт
                    if (i + 8 <= bitsToRemove) bytes[kk--] = 0;
                    //если битов меньше 8 то обнуляем только часть байта
                    if (i < bitsToRemove && i + 8 > bitsToRemove)
                    {
                        bytes[kk] >>= bitsToRemove - i + 1;
                        bytes[kk] <<= bitsToRemove - i + 1;
                    }
                }
                //проверяем подходит ли число
                BigInteger result = BigInteger.Abs(new BigInteger(bytes));
                if (bigN.CompareTo(result) > 0) return result;
            }
        }
    }
}
