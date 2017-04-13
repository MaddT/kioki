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
        //проверка электронной подписи RSA
        public static bool CheckDSASign(Tuple<string, BigInteger, BigInteger> input, Tuple<Tuple<BigInteger, BigInteger, BigInteger, BigInteger>, Tuple<BigInteger>> keys, BigInteger n)
        {
            BigInteger w;
            var sad = AsymmetricEncryption.AsymmetricEncryption.EuclidEx(keys.Item1.Item1, input.Item3);
            if (sad.Item2 < 0) w = sad.Item2 + keys.Item1.Item1;
            else w = sad.Item2;
            BigInteger hash = GetHash(input.Item1, n);
            BigInteger u1 = BigInteger.ModPow(BigInteger.Multiply(hash, w), 1, keys.Item1.Item1);
            BigInteger u2 = BigInteger.ModPow(BigInteger.Multiply(input.Item2, w), 1, keys.Item1.Item1);

            BigInteger v = BigInteger.ModPow(BigInteger.Multiply(BigInteger.ModPow(keys.Item1.Item3, u1, keys.Item1.Item2), BigInteger.ModPow(keys.Item1.Item4, u2, keys.Item1.Item2)), 1, keys.Item1.Item2);
            v = BigInteger.ModPow(v, 1, keys.Item1.Item1);

            return v == input.Item2;
        }

        //создание электронной подписи DSA
        public static Tuple<string, BigInteger, BigInteger> MakeDSASign(string source, Tuple<Tuple<BigInteger, BigInteger, BigInteger, BigInteger>, Tuple<BigInteger>> keys, BigInteger n)
        {
            BigInteger hash = GetHash(source, n);
            BigInteger k;
            BigInteger kMO;
            do
            {
                k = new Random().NextBigInteger(keys.Item1.Item1);
                var ress = AsymmetricEncryption.AsymmetricEncryption.EuclidEx(keys.Item1.Item1, k);
                if (ress.Item2 < 0) kMO = ress.Item2 + keys.Item1.Item1;
                else kMO = ress.Item2;
            } while (kMO.IsOne);
            

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

        public static BigInteger GetSimple()
        {
            Random rnd = new Random();
            byte[] bb = new byte[2];
            
            BigInteger big; 
            while (true)
            {
                rnd.NextBytes(bb);
                big = BigInteger.Abs(new BigInteger(bb));
                bool cond = false;
                for (BigInteger i = 2; i < big; i++)
                {
                    BigInteger renainder;
                    BigInteger.DivRem(big, i, out renainder);
                    if (renainder.IsZero) cond = true;
                }
                if (!cond) break;
            }

            return big;
        }
    }
}
