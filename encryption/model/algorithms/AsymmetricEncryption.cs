using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace encryption.model.AsymmetricEncryption
{
    public enum KeyAmount { b1024 = 1024, b2048 = 2048, b4096 = 4096 };

    public static class AsymmetricEncryption
    {
        

        //Решето Эратосфена
        public static int[] getSimplicityNumbers(int n)
        {
            int[] numbers = new int[n + 1];
            for (int i = 0; i < numbers.Length; i++)
                numbers[i] = i;
            numbers[0] = numbers[1] = -1;

            long p = 2;
            do
            {
                long k = p;
                for (long j = p * p; j <= n; j += p * (p == 2 ? 1 : 2))
                    numbers[j] = -1;

                for (long j = p + 1; j <= n; j++)
                    if (numbers[j] != -1)
                    {
                        p = j;
                        break;
                    }
                if (k == p) break;
            } while (true);

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
}
