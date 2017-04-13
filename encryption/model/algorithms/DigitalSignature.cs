using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using encryption.model.AsymmetricEncryption;
using System.Numerics;

namespace encryption.model.DigitalSignature
{
    public static class DigitalSignature
    {
        //создание электронной подписи DSA
        public static Tuple<string, BigInteger, BigInteger> MakeDSASign(string source, Tuple<Tuple<BigInteger, BigInteger, BigInteger, BigInteger>, Tuple<BigInteger>> keys, BigInteger n)
        {
            //Console.WriteLine("q : {0}, p : {1}, g : {2}, y : {3} ", keys.Item1.Item1, keys.Item1.Item2, keys.Item1.Item3, keys.Item1.Item4);
            //Console.WriteLine("x : {0}", keys.Item2.Item1);
            BigInteger hash = GetHash(source, n);
            BigInteger k = new Random().NextBigInteger(keys.Item1.Item1);
            BigInteger kMO;
            var ress = AsymmetricEncryption.AsymmetricEncryption.EuclidEx(keys.Item1.Item1, k);
            if (ress.Item2 < 0) kMO = ress.Item2 + keys.Item1.Item1;
            else kMO = ress.Item2;
            Console.WriteLine(kMO);

            BigInteger r = BigInteger.ModPow(BigInteger.ModPow(keys.Item1.Item3, k, keys.Item1.Item2), 1, keys.Item1.Item1);

            BigInteger s = BigInteger.ModPow(BigInteger.Multiply(kMO, BigInteger.Add(hash, BigInteger.Multiply(keys.Item2.Item1, r))), 1, keys.Item1.Item1);

            return new Tuple<string, BigInteger, BigInteger>(
                source, r, s
                );
        }

        //генерация ключей DSA
        public static Tuple<Tuple<BigInteger, BigInteger, BigInteger, BigInteger>, Tuple<BigInteger>> DSAKeys(KeyAmount b)
        {
            BigInteger q = AsymmetricEncryption.AsymmetricEncryption.GetSimpleNumber(b);
            BigInteger p;
            BigInteger reminder;
            BigInteger a = BigInteger.Zero;

            do
            {
                p = AsymmetricEncryption.AsymmetricEncryption.GetSimpleNumber((KeyAmount)((int)b * 2));
                BigInteger.DivRem(BigInteger.Subtract(p, BigInteger.One), q, out reminder);
                if (++a == 100)
                {
                    a = BigInteger.Zero;
                    q = AsymmetricEncryption.AsymmetricEncryption.GetSimpleNumber(b);
                }
            } while (!reminder.IsZero);

            Random rnd = new Random();
            BigInteger h = rnd.NextBigInteger(p - 3) + 2;
            BigInteger g;
            while (true)
            {
                g = BigInteger.ModPow(h, BigInteger.Divide(BigInteger.Subtract(p, BigInteger.One), q), p);
                if (!g.IsOne) break;
            }
            BigInteger x = rnd.NextBigInteger(q);
            BigInteger y = BigInteger.ModPow(g, x, p);

            return new Tuple<Tuple<BigInteger, BigInteger, BigInteger, BigInteger>, Tuple<BigInteger>>(
                new Tuple<BigInteger, BigInteger, BigInteger, BigInteger>(q, p, g, y),
                new Tuple<BigInteger>(x)
                );
        }

        //проверка электронной подписи RSA
        public static bool CheckRSASign(string source, BigInteger sign, Tuple<BigInteger, BigInteger> openKey, BigInteger n)
        {
            BigInteger hash = GetHash(source, n);
            BigInteger encS = AsymmetricEncryption.AsymmetricEncryption.RSADecrypt(sign, openKey);
            if (hash == encS) return true;
            else return false;
        }

        //создание электронной подписи RSA
        public static Tuple<string, BigInteger> MakeRSASign(string source, Tuple<BigInteger, BigInteger> closeKey, BigInteger n)
        {
            BigInteger hash = GetHash(source, n);
            BigInteger result = AsymmetricEncryption.AsymmetricEncryption.RSAEncrypt(hash, closeKey);
            return new Tuple<string, BigInteger>(source, result);
        }

        //хеш функция
        public static BigInteger GetHash(string s, BigInteger n)
        {
            BigInteger result = 150;
            for (int i = 0; i < s.Length; i++)
            {                
                result = BigInteger.ModPow(BigInteger.Add(result, new BigInteger(s[i])), 2, n);
            }

            return result;
        }

        public static Int16 GetSimple()
        {
            Random rnd = new Random();
            Int16 a, b;
            while (true)
            {
                a = (short)(rnd.Next(253) + 3);
                bool cond = false;
                for (int i = 2; i < a; i++)
                    if (a % i == 0) cond = true;
                if (!cond) break;
            }
            while (true)
            {
                b = (short)rnd.Next(256);
                bool cond = false;
                for (int i = 2; i < a; i++)
                    if (a % i == 0) cond = true;
                if (!cond) break;
            }

            return (short)(a * b);
        }
    }
}
