using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace encryption.model.AsymmetricEncryption
{
    public enum KeyAmount { b8 = 8, b16 = 16, b32 = 32, b64 = 64, b128 = 128, b256 = 256, b512 = 512, b1024 = 1024, b2048 = 2048 };

    public static class AsymmetricEncryption
    {
        private static Random rnd = new Random();

        //расшифровка El-Gamal - string
        public static string ElGamalDecrypt(BigInteger[] source, Tuple<BigInteger> key, Tuple<BigInteger, BigInteger, BigInteger> key1)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < source.Length / 2; i++)
                result.Append((char)(int)(BigInteger.ModPow(BigInteger.Multiply(BigInteger.ModPow(source[i * 2], BigInteger.Multiply(key.Item1, BigInteger.Subtract(key1.Item1, 2)), key1.Item1), source[i * 2 + 1]), 1, key1.Item1)));
            return result.ToString();
        }

        //шифрование El-Gamal - string
        public static BigInteger[] ElGamalEncrypt(string source, Tuple<BigInteger, BigInteger, BigInteger> key)
        {
            BigInteger[] result = new BigInteger[source.Length * 2];
            BigInteger k;
            char[] symbols = source.ToCharArray();
            for (int i = 0; i < symbols.Length; i++)
            {
                do
                {
                    k = rnd.NextBigInteger(key.Item1 - 3) + 2;
                } while (EuclidEx(k, key.Item1 - 1).Item3 != 1);
                result[2 * i] = BigInteger.ModPow(key.Item2, k, key.Item1);
                result[2 * i + 1] = BigInteger.ModPow(BigInteger.Multiply(BigInteger.ModPow(key.Item3, k, key.Item1), new BigInteger(symbols[i])), 1, key.Item1);
            }
            return result;
        }

        //генерация ключей для алгоритма Эль-Гамаля
        public static Tuple<Tuple<BigInteger, BigInteger, BigInteger>, Tuple<BigInteger>> GetElGamalKeys(KeyAmount b = KeyAmount.b1024)
        {
            BigInteger p = GetSimpleNumber(b);
            BigInteger g = GetPrimitiveRoot(p);
            BigInteger x = rnd.NextBigInteger(p - 3) + 2;
            BigInteger y = BigInteger.ModPow(g, x, p);
            return new Tuple<Tuple<BigInteger, BigInteger, BigInteger>, Tuple<BigInteger>>(
                new Tuple<BigInteger, BigInteger, BigInteger>(p, g, y),
                new Tuple<BigInteger>(x)
                );
        }

        //возвращает первообразный корень по модулю p
        private static BigInteger GetPrimitiveRoot(BigInteger p)
        {
            List<BigInteger> fact = new List<BigInteger>();
            BigInteger phi = BigInteger.Subtract(p, BigInteger.One);
            BigInteger n = phi;
            for (BigInteger i = 2; BigInteger.Multiply(i, i) <= n; i = BigInteger.Add(i, BigInteger.One))
            {
                BigInteger remainder;
                BigInteger.DivRem(n, i, out remainder);
                if (remainder.IsZero)
                {
                    fact.Add(i);
                    while (remainder.IsZero)
                    {
                        n = BigInteger.Divide(n, i);
                        BigInteger.DivRem(n, i, out remainder);
                    }
                }
            }

            if (n > BigInteger.One) fact.Add(n);

            for (BigInteger res = 2; res < p; res = BigInteger.Add(res, BigInteger.One))
            {
                bool ok = true;
                for (int i = 0; i < fact.Count() && ok; i++)
                    ok = ok && (BigInteger.ModPow(res, BigInteger.Divide(phi, fact[i]), p) != BigInteger.One);
                if (ok) return res;
            }

            return BigInteger.MinusOne;
        }

        //расшифровка RSA - string
        public static string RSADecrypt(BigInteger[] source, Tuple<BigInteger, BigInteger> key)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < source.Length; i++)
                result.Append((char)(int)BigInteger.ModPow(source[i], key.Item1, key.Item2));
            return result.ToString();
        }

        //расшифровка RSA - BigInteger
        public static BigInteger RSADecrypt(BigInteger source, Tuple<BigInteger, BigInteger> key)
        {
            BigInteger result = BigInteger.ModPow(source, key.Item1, key.Item2);
            return result;
        }

        //шифрование RSA - string
        public static BigInteger[] RSAEncrypt(string source, Tuple<BigInteger, BigInteger> key)
        {
            //StringBuilder result = new StringBuilder();
            BigInteger[] result = new BigInteger[source.Length];
            char[] symbols = source.ToCharArray();
            for (int i = 0; i < symbols.Length; i++)
                result[i] = BigInteger.ModPow(symbols[i], key.Item1, key.Item2);
            return result;
        }

        //шифрование RSA - BigInteger
        public static BigInteger RSAEncrypt(BigInteger source, Tuple<BigInteger, BigInteger> key)
        {
            BigInteger result = BigInteger.ModPow(source, key.Item1, key.Item2);
            return result;
        }

        //генерация ключей для алгоритма RSA
        public static Tuple<Tuple<BigInteger, BigInteger>, Tuple<BigInteger, BigInteger>> GetRSAKeys(KeyAmount b = KeyAmount.b1024)
        {
            BigInteger p = GetSimpleNumber(b);
            BigInteger q = GetSimpleNumber(b);
            BigInteger n = BigInteger.Multiply(p, q);
            BigInteger phin = BigInteger.Multiply(BigInteger.Subtract(p, BigInteger.One), BigInteger.Subtract(q, BigInteger.One));
            BigInteger d;
            BigInteger e;
            do
            {
                e = BigInteger.Add(rnd.NextBigInteger(BigInteger.Subtract(phin, BigInteger.One)), BigInteger.One);
                var nod = EuclidEx(phin, e);
                if (nod.Item3.IsOne)
                {
                    if (nod.Item2 < 0) d = BigInteger.Add(nod.Item2, phin);
                    else d = nod.Item2;
                    break;
                }
            } while (true);

            if (e == d) return GetRSAKeys(b);
            return new Tuple<Tuple<BigInteger, BigInteger>, Tuple<BigInteger, BigInteger>>(
                new Tuple<BigInteger, BigInteger>(e, n),        //open
                new Tuple<BigInteger, BigInteger>(d, n));       //close
        }

        //получить простое число, размерностью b бит
        public static BigInteger GetSimpleNumber(KeyAmount b = KeyAmount.b1024)
        {
            BigInteger result;
            while (true)
            {
                result = GetBigNumber(b);
                if (checkSimplicity(result, b)) break;
            }
            return result;
        }

        //алгоритм Евклида
        private static Tuple<BigInteger, BigInteger, BigInteger> EuclidEx(BigInteger a, BigInteger b)
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

        //генерация большого числа, размеров определенного количества бит
        public static BigInteger GetBigNumber(KeyAmount keyAmount)
        {
            int nBits = (int)keyAmount;
            byte[] bytes = new byte[nBits / 8];
            rnd.NextBytes(bytes);
            return BigInteger.Abs(new BigInteger(bytes));
        }

        //проверка простоты методом Миллера — Рабина
        public static bool checkSimplicity(BigInteger n, KeyAmount keyAmount)
        {
            int k = (int)keyAmount;         //размерность ключа
            //исключаем числа делимые на простые числа от 2 до 256 либо к
            int[] simpleNumberForCheck = getSimplicityNumbers(k <= 256 ? 256 : k);
            for (int i = 0; i < simpleNumberForCheck.Length; i++)
            {
                BigInteger remainder;
                BigInteger.DivRem(n, new BigInteger(simpleNumberForCheck[i]), out remainder);
                if (remainder.IsZero || n.CompareTo(new BigInteger(simpleNumberForCheck[i])) == 0) return false;
            }

            Random rnd = new Random();
            int s = 0;
            BigInteger nmm = BigInteger.Subtract(n, BigInteger.One);
            BigInteger t = nmm;
            //вычисляем коэффициенты t и s
            do
            {
                t = BigInteger.Divide(t, new BigInteger(2));
                s++;
                BigInteger remainder;
                BigInteger.DivRem(t, new BigInteger(2), out remainder);
                if (remainder != 0) break;
            } while (true);

            //проверяем условия простоты
            int kk = 30720 / k;
            for (int i = 0; i < kk; i++)
            {
                BigInteger a;
                for (;;)
                {
                    a = rnd.NextBigInteger(n);
                    if (nmm.CompareTo(a) > 0 && a.CompareTo(new BigInteger(2)) >= 0) break;
                }
                //проверяем сравнимость по модулю
                BigInteger x = BigInteger.ModPow(a, t, n);
                if (x.IsOne || x.CompareTo(nmm) == 0) continue;
                for (int j = 1; j < s; j++)
                {
                    x = BigInteger.ModPow(x, new BigInteger(2), n);
                    if (x.IsOne) return false;                      //составное
                    if (x.CompareTo(nmm) == 0) goto ff;             //перейти на следующую проверку
                }
                return false;                                       //составное
            ff:;
            }
            return true;
        }

        //Решето Эратосфена
        private static int[] getSimplicityNumbers(int n)
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
                int kk = bytes.Length - 1;                          //индекс обрабатываемого байта
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
