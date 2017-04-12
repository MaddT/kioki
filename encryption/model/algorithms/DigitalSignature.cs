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
