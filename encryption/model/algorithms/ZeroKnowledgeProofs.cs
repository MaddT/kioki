﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace encryption.model.ZeroKnowledgeProofs
{
    public static class ZeroKnowledgeProofs
    {
        private static Random rnd1 = new Random();
        private static Random rnd2 = new Random();

        //четвертый этап
        public static bool FiatShamirFourth(long e, long y, long x, Tuple<long, long> key)
        {
            long y2 = (long)BigInteger.ModPow(x * (long)Math.Pow(key.Item1, e), 1, key.Item2);
            if (y2 == (long)BigInteger.ModPow(y, 2, key.Item2)) return true;

            return false;
        }

        //третий этап
        public static long FiatShamirThird(long e, long r, Tuple<Tuple<long, long>, Tuple<long>> key)
        {
            long y;
            y = (long)BigInteger.ModPow(r * (long)Math.Pow(key.Item2.Item1, e), 1, key.Item1.Item2);
            return y;
        }
        //второй этап
        public static long FiatShamirSecond()
        {
            return rnd1.Next(2);
        }

        //первый этап проверки
        public static Tuple<long, long> FiatShamirFirst(Tuple<long, long> key)
        {
            long r;
            r = rnd1.Next((int)key.Item2 - 1) + 1;

            long x = (long)BigInteger.ModPow(r, 2, key.Item2);

            return new Tuple<long, long>(r, x);
        }

        //генерация ключей протокола Фиата-Шамира
        public static Tuple<Tuple<long, long>, Tuple<long>> FiatShamirKeys()
        {
            int p = 0, q = 0;
            long[] array = getSimplicityNumbers(10000);
            do
            {
                p = (int)array[rnd1.Next(array.Length)];
                q = (int)array[rnd1.Next(array.Length / 2)];
                if (p > q) break;
            } while (true);
            long n = (long)p * (long)q;
            long s;
            do
            {
                s = rnd1.Next((int)n - 1) + 1;
                var aaa = EuclidEx(n, s);
                if (aaa.Item3 == 1) break;
            } while (true);
            long v = (long)BigInteger.ModPow(s, 2, n);


            return new Tuple<Tuple<long, long>, Tuple<long>>(
                new Tuple<long, long>(v, n),
                new Tuple<long>(s));
        }

        //Решето Эратосфена
        private static long[] getSimplicityNumbers(long n)
        {
            //массив чисел от 0 до n включительно
            long[] numbers = new long[n + 1];
            for (long i = 0; i < numbers.Length; i++)
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
            long k1 = 0;
            for (long i = 0; i < numbers.Length; i++)
                if (numbers[i] != -1) k1++;

            long[] result = new long[k1];

            k1 = 0;
            for (long i = 0; i < numbers.Length; i++)
                if (numbers[i] != -1) result[k1++] = numbers[i];

            return result;
        }

        //алгоритм Евклида
        public static Tuple<BigInteger, BigInteger, BigInteger> EuclidEx(BigInteger a, BigInteger b)
        {
            BigInteger d0 = a;
            BigInteger d1 = b;
            BigInteger x0 = 1;
            BigInteger x1 = 0;
            BigInteger y0 = 0;
            BigInteger y1 = 1;
            while (d1 > 1)
            {
                BigInteger q = BigInteger.Divide(d0, d1);
                BigInteger d2;
                BigInteger.DivRem(d0, d1, out d2);
                BigInteger x2 = BigInteger.Subtract(x0, BigInteger.Multiply(q, x1));
                BigInteger y2 = BigInteger.Subtract(y0, BigInteger.Multiply(q, y1));
                d0 = d1; d1 = d2;
                x0 = x1; x1 = x2;
                y0 = y1; y1 = y2;
            }
            return new Tuple<BigInteger, BigInteger, BigInteger>(x1, y1, d1);
        }

        //функция эйлера для натуральных чисел
        public static double PHI(long n)
        {
            if (Prime(n)) return n - 1;

            long[] simples = getSimplicityNumbers(n / 2 + 1);

            //нахождение простых множителей
            List<long[]> list = new List<long[]>();
            int i = 0;
            while (true)
            {
                if (n % simples[i] == 0)
                {
                    int k;
                    for (k = 1; k < n; k++)
                    {
                        n /= simples[i];
                        if (n % simples[i] != 0) break;
                    }
                    list.Add(new long[] { simples[i], k });
                }
                i++;
                if (i >= simples.Length) break;
            }

            //вычисление ф-ии эйлера
            double res = 1;
            foreach (long[] item in list)
            {
                res *= Math.Pow(item[0], item[1]) - Math.Pow(item[0], item[1] - 1);
                Console.WriteLine(item[0] + " - " + item[1]);
            }

            return res;
        }

        //проверка на простоту
        public static bool Prime(long n)
        {
            for (int i = 2; i < n / 2; i++)
                if (n % i == 0) return false;

            return true;
        }
    }
}
