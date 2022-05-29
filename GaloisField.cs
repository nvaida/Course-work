using System;

namespace Course_work
{
    /**
     * 
     * Математические операции в поле Галуа
     * 
     */
    public sealed class GaloisField
    {

        private int[] expTable;
        private int[] logTable;
        private GaloisFieldPoly zero;
        private GaloisFieldPoly one;
        private readonly int size;
        private readonly int primitive;
        private readonly int generatorBase;

        /**
         * Создает и инициализирует поле Галуа
         * {primitive} - неприводимый полином, представленный битами. где младший бит - нулевая степень
         * {size} - размерность поля Галуа
         * {genBase} - Множитель b в генерирующем полиноме. Может быть 0 или 1. (g(x) = (x+a^b)(x+a^(b+1))...(x+a^(b+2t-1)))
         * 
         */
        public GaloisField(int primitive, int size, int genBase)
        {
            this.primitive = primitive;
            this.size = size;
            this.generatorBase = genBase;

            expTable = new int[size];
            logTable = new int[size];
            int x = 1;
            for (int i = 0; i < size; i++)
            {
                expTable[i] = x;
                x <<= 1;
                if (x >= size)
                {
                    x ^= primitive;
                    x &= size - 1;
                }
            }
            for (int i = 0; i < size - 1; i++)
            {
                logTable[expTable[i]] = i;
            }
            // logTable[0] == 0 but this should never be used
            zero = new GaloisFieldPoly(this, new int[] { 0 });
            one = new GaloisFieldPoly(this, new int[] { 1 });
        }

        internal GaloisFieldPoly Zero
        {
            get
            {
                return zero;
            }
        }

        internal GaloisFieldPoly One
        {
            get
            {
                return one;
            }
        }

        // Создает одночлен: coefficient * x^degree
        internal GaloisFieldPoly buildMonomial(int degree, int coefficient)
        {
            if (degree < 0)
            {
                throw new ArgumentException();
            }
            if (coefficient == 0)
            {
                return zero;
            }
            int[] coefficients = new int[degree + 1];
            coefficients[0] = coefficient;
            return new GaloisFieldPoly(this, coefficients);
        }

        // Сложение или вычитания - равнозначная операция в поле Галуа
        static internal int addOrSubtract(int a, int b)
        {
            return a ^ b;
        }

        // 2 в степени {a} в поле Галуа
        internal int exp(int a)
        {
            return expTable[a];
        }

        // логарифм по основанию 2 в поле Галуа
        internal int log(int a)
        {
            if (a == 0)
            {
                throw new ArgumentException();
            }
            return logTable[a];
        }

        // 1/a или a^(-1)
        internal int inverse(int a)
        {
            if (a == 0)
            {
                throw new ArithmeticException();
            }
            return expTable[size - logTable[a] - 1];
        }

        // Умножение a * b в поле Галуа
        internal int multiply(int a, int b)
        {
            if (a == 0 || b == 0)
            {
                return 0;
            }
            return expTable[(logTable[a] + logTable[b]) % (size - 1)];
        }

        // Размерность поля
        public int Size
        {
            get { return size; }
        }


        // Множитель в генерирующем полиноме
        public int GeneratorBase
        {
            get { return generatorBase; }
        }

    }
}
