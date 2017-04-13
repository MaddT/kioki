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
        //генерация ключей DSA
        public static Tuple<Tuple<BigInteger, BigInteger, BigInteger>, Tuple<BigInteger>> DSAKeys(KeyAmount b)
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

            return new Tuple<Tuple<BigInteger, BigInteger, BigInteger>, Tuple<BigInteger>>(
                new Tuple<BigInteger, BigInteger, BigInteger>(q, p, y),
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
    }
}
