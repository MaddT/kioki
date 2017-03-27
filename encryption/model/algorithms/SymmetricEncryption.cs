using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace encryption.SymmetricEncryption
{
    public enum AlphabetForEncryption { Latin, Cyrillic };

    public static class EncryptSymmetric
    {
        #region Метод "Железнодорожной изгороди"
        public static string ZigZagCipher(string input, sbyte key)
        {
            StringBuilder result = new StringBuilder();
            int zzLong = 2 * key - 2;                       //расстояние между символами одного ряда
            int shift = zzLong * 2;                         //смещение позиции считывания
            int symbolIndex;                                //позиция добавляемого символа

            //проходим по рядам
            for (int row = 0; row < key; row++)
            {
                symbolIndex = row;
                while (true)
                {
                    //добавляем первый символ
                    if (symbolIndex < input.Length)
                        result.Append(input[symbolIndex]);
                    else break;
                    //если нижний ряд
                    if (zzLong == 0)
                    {
                        symbolIndex += shift;
                        continue;
                    }
                    //добавляем второй символ
                    if (symbolIndex + zzLong < input.Length)
                        result.Append(input[symbolIndex + zzLong]);
                    else break;
                    //смещаем позицию считывания
                    symbolIndex += shift;
                }
                //уменьшаем расстояние между символами
                zzLong -= 2;
                //снижаем сдвиг если первый ряд
                shift /= (row == 0 ? 2 : 1);
            }

            return result.ToString();
        }

        public static string ZigZagDeCipher(string input, sbyte key)
        {
            StringBuilder result = new StringBuilder();
            result.Append('_', input.Length);
            int zzLong = 2 * key - 2;                       //расстояние между символами одного ряда
            int shift = zzLong * 2;                         //смещение позиции записи
            int symbolIndex;                                //позиция добавляемого символа
            int readingSymbol = 0;                          //индекс считываемого символа

            //записываем по рядам
            for (int row = 0; row < key; row++)
            {
                symbolIndex = row;
                while (true)
                {
                    //первый символ
                    if (symbolIndex < result.Length)
                        result[symbolIndex] = input[readingSymbol++];
                    else break;
                    //если нижний ряд
                    if (zzLong == 0)
                    {
                        symbolIndex += shift;
                        continue;
                    }
                    //второй символ
                    if (symbolIndex + zzLong < input.Length)
                        result[symbolIndex + zzLong] = input[readingSymbol++];
                    else break;
                    //смещаем позицию записи
                    symbolIndex += shift;
                }
                //уменьшаем расстояние между символами
                zzLong -= 2;
                //снижаем сдвиг если первый ряд
                shift /= (row == 0 ? 2 : 1);
            }

            return result.ToString();
        }
        #endregion

        #region "Столбцовый метод"
        public static string ColumnCipher(string input, string key)
        {
            byte[] keyNumbers = getKeyNumbers(key);

            //формируем зашифрованную строку
            StringBuilder result = new StringBuilder();
            for (int keyNumber = 1; keyNumber <= keyNumbers.Length; keyNumber++)
            {
                //определяем номер необходимого столбца
                int columnForAdding;
                for (columnForAdding = 0; columnForAdding < keyNumbers.Length; columnForAdding++)
                    if (keyNumbers[columnForAdding] == keyNumber) break;
                //добаляем символы в результирующую строку
                while (true)
                    if (columnForAdding < input.Length)
                    {
                        result.Append(input[columnForAdding]);
                        columnForAdding += keyNumbers.Length;
                    }
                    else break;
            }

            return result.ToString();
        }

        public static string ColumnDeCipher(string input, string key)
        {
            byte[] keyNumbers = getKeyNumbers(key);

            //восстанавливаем информацию
            StringBuilder result = new StringBuilder();
            result.Append('_', input.Length);
            int currentSymbol = 0;                                  //индекс записываемого символа
            for (int columnForWriting = 1; columnForWriting <= keyNumbers.Length; columnForWriting++)
            {
                //находим необходимое смещение
                int shift;
                for (shift = 0; shift < keyNumbers.Length; shift++)
                    if (keyNumbers[shift] == columnForWriting) break;
                //записываем символы в нужную позицию
                while (true)
                {
                    if (shift < result.Length)
                        result[shift] = input[currentSymbol++];
                    else break;
                    shift += keyNumbers.Length;
                }
            }

            return result.ToString();
        }
        #endregion

        #region Метод "Поворотной решетки"
        public static string TurnedGridCipher(string input, byte[,] grid, string key)
        {
            byte[] keyNumbers = getKeyNumbers(key);
            byte[,] turnedGrid = (byte[,])grid.Clone();
            char[,] resultGrid = new char[grid.GetLength(0), grid.GetLength(1)];
            int symbToEncrypt = grid.GetLength(0) * grid.GetLength(1);      //кол-во символов в одной матрице
            StringBuilder result = new StringBuilder();

            //формируем шифрованное сообщение
            int substrCounter = 0;              //номер подстроки
            int symbNumber = 0;                 //номер символа, читаемого в подстроке
            while (true)
            {
                //получаем обрабатываемую подстроку
                string buff;
                if (substrCounter * symbToEncrypt >= input.Length) break;
                buff = input.Substring(substrCounter * symbToEncrypt,
                     (substrCounter * symbToEncrypt + symbToEncrypt >= input.Length ?
                        input.Length - substrCounter * symbToEncrypt :
                        symbToEncrypt));

                //записываем символы подстроки в матрицу
                for (int turn = 0; turn < 4; turn++)
                {
                    //записываем символы в решетку
                    for (int i = 0; i < grid.GetLength(0); i++)
                        for (int j = 0; j < grid.GetLength(1); j++)
                        {
                            if (turnedGrid[i, j] == 1)
                                resultGrid[i, j] = buff[symbNumber++];
                            //если подстрока закончилась
                            if (symbNumber >= buff.Length) goto ExitLoops;
                        }

                    //поворачиваем решетку
                    turnedGrid = turnGrid(turnedGrid);
                }
            ExitLoops:;

                //формируем шифровку
                for (int keyN = 1; keyN <= key.Length; keyN++)
                {
                    int column;
                    for (column = 0; column < keyNumbers.Length; column++)
                        if (keyN == keyNumbers[column]) break;
                    for (int row = 0; row < resultGrid.GetLength(0); row++)
                        if (resultGrid[row, column] != '\0') result.Append(resultGrid[row, column]);
                }

                //очищаем матрицу
                for (int i = 0; i < resultGrid.GetLength(0); i++)
                    for (int j = 0; j < resultGrid.GetLength(1); j++)
                        resultGrid[i, j] = '\0';
                turnedGrid = (byte[,])grid.Clone();
                //переходим на следующую подстроку
                substrCounter++;
                symbNumber = 0;
            }

            return result.ToString();
        }

        public static string TurnedGridDeCipher(string input, byte[,] grid, string key)
        {
            byte[] keyNumbers = getKeyNumbers(key);
            byte[,] turnedGrid = (byte[,])grid.Clone();
            char[,] resultGrid = new char[grid.GetLength(0), grid.GetLength(1)];
            StringBuilder result = new StringBuilder();
            int symbToEncrypt = grid.GetLength(0) * grid.GetLength(1);      //кол-во символов в одной матрице

            //дешифруем сообщение
            int substrCounter = 0;              //номер подстроки
            while (true)
            {
                //получаем обрабатываемую подстроку
                string buff;
                if (substrCounter * symbToEncrypt >= input.Length) break;
                buff = input.Substring(substrCounter * symbToEncrypt,
                     (substrCounter * symbToEncrypt + symbToEncrypt >= input.Length ?
                        input.Length - substrCounter * symbToEncrypt :
                        symbToEncrypt));

                int keyN = 1;          //индекс ключевого символа
                //записываем в матрицу шифрованную информацию
                if (buff.Length == symbToEncrypt)
                {       //если подстрока размером с матрицу
                    for (int col = 0; col < resultGrid.GetLength(1); col++)
                    {
                        int column;
                        for (column = 0; column < keyNumbers.Length; column++)
                            if (keyN == keyNumbers[column]) break;
                        string subBuff = buff.Substring(resultGrid.GetLength(0) * col, resultGrid.GetLength(0));
                        //записываем в столбец
                        for (int row = 0; row < resultGrid.GetLength(0); row++)
                            resultGrid[row, column] = subBuff[row];
                        keyN++;
                    }
                }
                else
                {       //если подстрока не полностью заполняет матрицу
                    int symbCounter = 0;
                    byte[,] encrGrid = new byte[grid.GetLength(0), grid.GetLength(1)];
                    //помечаем клетки с символами
                    for (int turn = 0; turn < 4; turn++)
                    {
                        for (int i = 0; i < turnedGrid.GetLength(0); i++)
                            for (int j = 0; j < turnedGrid.GetLength(1); j++)
                                if (turnedGrid[i, j] == 1 && symbCounter < buff.Length)
                                {
                                    encrGrid[i, j] = 1;
                                    symbCounter++;
                                }
                                else if (symbCounter >= buff.Length) goto ExitLoops;

                        //поворачиваем матрицу
                        turnedGrid = turnGrid(turnedGrid);
                    }
                ExitLoops:;

                    //записываем имеющиеся символы
                    symbCounter = 0;
                    for (int col = 0; col < resultGrid.GetLength(1); col++)
                    {
                        int column;
                        for (column = 0; column < keyNumbers.Length; column++)
                            if (keyN == keyNumbers[column]) break;
                        //записываем в столбец
                        for (int row = 0; row < resultGrid.GetLength(0); row++)
                            if (encrGrid[row, column] == 1 && symbCounter < buff.Length)
                                resultGrid[row, column] = buff[symbCounter++];
                        keyN++;
                    }
                }

                //читаем при помощи матрицы Кардано
                turnedGrid = (byte[,])grid.Clone();
                for (int turn = 0; turn < 4; turn++)
                {
                    for (int i = 0; i < turnedGrid.GetLength(0); i++)
                        for (int j = 0; j < turnedGrid.GetLength(1); j++)
                            if (turnedGrid[i, j] == 1 && resultGrid[i, j] != '\0')
                                result.Append(resultGrid[i, j]);
                    turnedGrid = turnGrid(turnedGrid);
                }

                //очищаем матрицу
                for (int i = 0; i < resultGrid.GetLength(0); i++)
                    for (int j = 0; j < resultGrid.GetLength(1); j++)
                        resultGrid[i, j] = '\0';
                //следующая подстрока
                substrCounter++;
            }

            return result.ToString();
        }
        #endregion

        #region Шифр Цезаря
        public static string CaesarCipher(string source, int key, AlphabetForEncryption alph = AlphabetForEncryption.Latin)
        {
            int symbolsQuantity = (alph == AlphabetForEncryption.Latin) ? 26 : 33;       //размерность алфавита
            int aCode = (alph == AlphabetForEncryption.Latin) ? (int)'a' : (int)'а';     //код начала алфавита
            char[] result = new char[source.Length];

            //заменяем символы
            for (int i = 0; i < source.Length; i++)
            {
                int orderInAlphabet = (int)source[i] - aCode;
                orderInAlphabet = (orderInAlphabet + key) % symbolsQuantity;
                result[i] = (char)(aCode + orderInAlphabet);
            }

            return new String(result);
        }

        public static string CaesarDeCipher(string source, int key, AlphabetForEncryption alph = AlphabetForEncryption.Latin)
        {
            int symbolsQuantity = (alph == AlphabetForEncryption.Latin) ? 26 : 33;       //размерность алфавита
            int aCode = (alph == AlphabetForEncryption.Latin) ? (int)'a' : (int)'а';     //код начала алфавита
            char[] result = new char[source.Length];

            //восстанавливаем символы
            for (int i = 0; i < source.Length; i++)
            {
                int orderInAlphabet = (int)source[i] - aCode;
                orderInAlphabet = (orderInAlphabet + symbolsQuantity - key) % symbolsQuantity;
                result[i] = (char)(aCode + orderInAlphabet);
            }

            return new String(result);
        }
        #endregion

        #region Шифр Плейфера
        public static string PlayfairCipher(string input, string key)
        {
            StringBuilder result = new StringBuilder();
            char[,] matrix = getPlayfairMatrix(key);

            StringBuilder source = new StringBuilder(input);
            int symbN = 0;
            while (true)
            {
                if (source[symbN] == source[symbN + 1])
                    source.Insert(symbN + 1, 'X');                
                symbN += 2;
                if (symbN + 1 >= source.Length) break;
            }
            if (source.Length % 2 != 0) source.Append('X');
            source.Replace('J', 'I');
            input = source.ToString();

            //формирование зашифрованной строки
            for (int m = 0; m < input.Length; m += 2)
            {
                //определяем координаты символов в матрице
                //(mx1, my1) - первый символ
                //(mx2, my2) - второй символ
                int mx1, my1, mx2, my2;
                mx1 = my1 = mx2 = my2 = -1;
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (matrix[i, j] == input[m])
                        { mx1 = j; my1 = i; }
                        if (matrix[i, j] == input[m + 1])
                        { mx2 = j; my2 = i; }
                        if (mx1 != -1 && mx2 != -1) goto ExitLoops;
                    }
                }
            ExitLoops:;

                //шифрование
                //если в одной строке
                if (my1 == my2)
                {
                    if (mx1 + 1 < matrix.GetLength(1)) result.Append(matrix[my1, mx1 + 1]);
                    else result.Append(matrix[my1, 0]);
                    if (mx2 + 1 < matrix.GetLength(1)) result.Append(matrix[my2, mx2 + 1]);
                    else result.Append(matrix[my2, 0]);
                    continue;
                }
                //если в одном столбце
                if (mx1 == mx2)
                {
                    if (my1 + 1 < matrix.GetLength(0)) result.Append(matrix[my1 + 1, mx1]);
                    else result.Append(matrix[0, mx1]);
                    if (my2 + 1 < matrix.GetLength(0)) result.Append(matrix[my2 + 1, mx2]);
                    else result.Append(matrix[0, mx2]);
                    continue;
                }
                //если в разных строках и столбцах
                result.Append(matrix[my1, mx2]);
                result.Append(matrix[my2, mx1]);
            }

            return result.ToString();
        }

        public static string PlayfairDeCipher(string input, string key)
        {
            StringBuilder result = new StringBuilder();
            char[,] matrix = getPlayfairMatrix(key);

            //дешифровка строки
            for (int m = 0; m < input.Length; m += 2)
            {
                //определяем координаты символов в матрице
                //(mx1, my1) - первый символ
                //(mx2, my2) - второй символ
                int mx1, my1, mx2, my2;
                mx1 = my1 = mx2 = my2 = -1;
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (matrix[i, j] == input[m])
                        { mx1 = j; my1 = i; }
                        if (matrix[i, j] == input[m + 1])
                        { mx2 = j; my2 = i; }
                        if (mx1 != -1 && mx2 != -1) goto ExitLoops;
                    }
                }
            ExitLoops:;

                //инверсия правил
                //если в одной строке
                if (my1 == my2)
                {
                    if (mx1 - 1 >= 0) result.Append(matrix[my1, mx1 - 1]);
                    else result.Append(matrix[my1, matrix.GetLength(1) - 1]);
                    if (mx2 - 1 >= 0) result.Append(matrix[my2, mx2 - 1]);
                    else result.Append(matrix[my2, matrix.GetLength(1) - 1]);
                    continue;
                }
                //если в одном столбце
                if (mx1 == mx2)
                {
                    if (my1 - 1 >= 0) result.Append(matrix[my1 - 1, mx1]);
                    else result.Append(matrix[matrix.GetLength(0) - 1, mx1]);
                    if (my2 - 1 >= 0) result.Append(matrix[my2 - 1, mx2]);
                    else result.Append(matrix[matrix.GetLength(0) - 1, mx2]);
                    continue;
                }
                //если в разных строках и столбцах
                result.Append(matrix[my1, mx2]);
                result.Append(matrix[my2, mx1]);
            }
            
            int symbN = 1;
            while(true)
            {
                if (result[symbN] == 'X' && result[symbN - 1] == result[symbN + 1])
                    result.Remove(symbN, symbN);
                symbN++;
                if (symbN + 1 >= result.Length) break;
            }

            return result.ToString();
        }
        #endregion

        #region Шифр Виженера
        public static string VigenereCipher(string input, string key)
        {
            StringBuilder result = new StringBuilder();

            //модифицируем ключ
            do
            {
                key += key;
                if (key.Length > input.Length)
                {
                    key = key.Substring(0, input.Length);
                    break;
                }
            } while (true);

            int alphabetSize = ('Z' - 'A' + 1);
            char firstLetterCode = 'A';
            //шифруем информацию
            for (int i = 0; i < input.Length; i++)
            {
                int chSource = input[i] - firstLetterCode;              //символ исходного сообщения
                int chKey = key[i] - firstLetterCode;                   //ключевой символ
                int chRes = (chSource + chKey) % alphabetSize;
                result.Append((char)(chRes + firstLetterCode));
            }

            return result.ToString();
        }

        public static string VigenereDeCipher(string input, string key)
        {
            StringBuilder result = new StringBuilder();
            //модифицируем ключ
            do
            {
                key += key;
                if (key.Length > input.Length)
                {
                    key = key.Substring(0, input.Length);
                    break;
                }
            } while (true);

            int alphabetSize = ('Z' - 'A' + 1);
            char firstLetter = 'A';
            //дешифруем информацию
            for (int i = 0; i < input.Length; i++)
            {
                int chSource = input[i] - firstLetter;              //символ исходного сообщения
                int chKey = key[i] - firstLetter;                   //ключевой символ
                int chRes = (chSource - chKey + alphabetSize) % alphabetSize;
                result.Append((char)(chRes + firstLetter));
            }

            return result.ToString();
        }
        #endregion

        //поворот матрицы
        private static byte[,] turnGrid(byte[,] grid)
        {
            byte[,] turnedGrid = new byte[grid.GetLength(0), grid.GetLength(1)];

            //транспонируем
            for (int i = 0; i < turnedGrid.GetLength(0); i++)
                for (int j = 0; j < turnedGrid.GetLength(1); j++)
                    turnedGrid[i, j] = grid[j, i];

            //инвертируем строки
            for (int i = 0; i < turnedGrid.GetLength(0); i++)
                for (int j = 0; j < turnedGrid.GetLength(1) / 2; j++)
                {
                    byte buff = turnedGrid[i, j];
                    turnedGrid[i, j] = turnedGrid[i, turnedGrid.GetLength(1) - 1 - j];
                    turnedGrid[i, turnedGrid.GetLength(1) - 1 - j] = buff;
                }

            return turnedGrid;
        }

        //формирование индексов символов ключа
        private static byte[] getKeyNumbers(string key)
        {
            char[] keySymbols = key.ToCharArray();                  //символы ключа
            byte[] keyNumbers = new byte[keySymbols.Length];        //номера символов ключа

            //инициализируем 1-ами
            for (int i = 0; i < keyNumbers.Length; i++) keyNumbers[i]++;
            //пронумеруем символы ключа
            for (int i = 1; i < keySymbols.Length; i++)
                for (int j = i - 1; j >= 0; j--)
                    if (keySymbols[i] >= keySymbols[j])
                        keyNumbers[i]++;
                    else
                        keyNumbers[j]++;
            return keyNumbers;
        }

        //создает матрицу для шифрования методом Плейфера
        private static char[,] getPlayfairMatrix(string key)
        {
            List<char> keySymbols = new List<char>();   //список уникальных символов ключа

            //анализ ключа
            foreach (char ch in key)
                if (!keySymbols.Where(c => c == ch).Any()) keySymbols.Add(ch);

            //построение матрицы
            char[,] matrix = new char[5, 5];        //матрица Плейфера
            int keyCounter = 0;                     //количество добавленный символов ключа
            char addedSymbol = 'A';                 //хранит символ, добавляемый в матрицу
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    //добавляем символы из ключа
                    if (keyCounter < keySymbols.Count)
                    {
                        matrix[i, j] = keySymbols[keyCounter++];
                        continue;
                    }
                    //добавляем символы из алфавита, которых нет в ключе
                    while (true)
                    {
                        //j и i находятся в обной ячейке
                        if (addedSymbol == 'J') { addedSymbol++; continue; }
                        if (!keySymbols.Where(c => c == addedSymbol).Any())
                        {
                            matrix[i, j] = addedSymbol++;
                            break;
                        }
                        addedSymbol++;
                    }
                }

            return matrix;
        }
    }
}
