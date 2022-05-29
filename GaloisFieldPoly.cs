using System;

namespace Course_work
{
    /**
     * Операции с полиномом
     */
    internal sealed class GaloisFieldPoly
    {
        private readonly GaloisField field;
        private readonly int[] coefficients;

        /**
         * 
         * field - Экземпляр поля для выполнения вычислений
         * coefficients - Коэффициенты полинома в порядке убывания степеней элементов
         * 
         */
        internal GaloisFieldPoly(GaloisField field, int[] coefficients)
        {
            if (coefficients.Length == 0)
            {
                throw new ArgumentException("Полином не должен быть пустым!");
            }
            this.field = field;
            int coefficientsLength = coefficients.Length;
            // Первый элемент должен иметь не нулевое значение
            if ((coefficientsLength > 1) && (coefficients[0] == 0))
            {
                int firstNonZero = 1;
                while ((firstNonZero < coefficientsLength) && (coefficients[firstNonZero] == 0))
                {
                    firstNonZero++;
                }
                if (firstNonZero == coefficientsLength)
                {
                    this.coefficients = new int[] { 0 };
                }
                else
                {
                    this.coefficients = new int[coefficientsLength - firstNonZero];
                    Array.Copy(coefficients,
                        firstNonZero,
                        this.coefficients,
                        0,
                        this.coefficients.Length);
                }
            }
            else
            {
                this.coefficients = coefficients;
            }
        }

        // Коэффициенты полинома в порядке убывания степеней элементов
        internal int[] Coefficients
        {
            get { return coefficients; }
        }

        // Степень полинома
        internal int Degree
        {
            get
            {
                return coefficients.Length - 1;
            }
        }

        // Возвращает признак нулевого полинома - первый элемент равен 0
        internal bool isZero
        {
            get { return coefficients[0] == 0; }
        }

        // Возвращает коэфициент для заданной {degree} степени
        internal int getCoefficient(int degree)
        {
            return coefficients[coefficients.Length - 1 - degree];
        }

        // Вычисление полинома для заданного значения
        internal int evaluateAt(int a)
        {
            int result = 0;
            if (a == 0)
            {
                // Если x = 0, то результат = коэффициент при x^0 - остальные элементы будут нули
                return getCoefficient(0);
            }
            int size = coefficients.Length;
            if (a == 1)
            {
                // Если x = 1, то сумма коэффициентов
                foreach (var coefficient in coefficients)
                {
                    result = GaloisField.addOrSubtract(result, coefficient);
                }
                return result;
            }
            result = coefficients[0];
            for (int i = 1; i < size; i++)
            {
                result = GaloisField.addOrSubtract(field.multiply(a, result), coefficients[i]);
            }
            return result;
        }

        // Сложение или вычитание (равнозначная операция) полиномов 
        internal GaloisFieldPoly addOrSubtract(GaloisFieldPoly other)
        {
            if (!field.Equals(other.field))
            {
                throw new ArgumentException("Полиномы должны относится к одному полю");
            }
            if (isZero)
            {
                return other;
            }
            if (other.isZero)
            {
                return this;
            }

            int[] smallerCoefficients = this.coefficients;
            int[] largerCoefficients = other.coefficients;
            if (smallerCoefficients.Length > largerCoefficients.Length)
            {
                int[] temp = smallerCoefficients;
                smallerCoefficients = largerCoefficients;
                largerCoefficients = temp;
            }
            int[] sumDiff = new int[largerCoefficients.Length];
            int lengthDiff = largerCoefficients.Length - smallerCoefficients.Length;
            Array.Copy(largerCoefficients, 0, sumDiff, 0, lengthDiff);

            for (int i = lengthDiff; i < largerCoefficients.Length; i++)
            {
                sumDiff[i] = GaloisField.addOrSubtract(smallerCoefficients[i - lengthDiff], largerCoefficients[i]);
            }

            return new GaloisFieldPoly(field, sumDiff);
        }

        // Перемножение полиномов
        internal GaloisFieldPoly multiply(GaloisFieldPoly other)
        {
            if (!field.Equals(other.field))
            {
                throw new ArgumentException("Полиномы должны относится к одному полю");
            }
            if (isZero || other.isZero)
            {
                return field.Zero;
            }
            int[] aCoefficients = this.coefficients;
            int aLength = aCoefficients.Length;
            int[] bCoefficients = other.coefficients;
            int bLength = bCoefficients.Length;
            int[] product = new int[aLength + bLength - 1];
            for (int i = 0; i < aLength; i++)
            {
                int aCoeff = aCoefficients[i];
                for (int j = 0; j < bLength; j++)
                {
                    product[i + j] = GaloisField.addOrSubtract(product[i + j],
                        field.multiply(aCoeff, bCoefficients[j]));
                }
            }
            return new GaloisFieldPoly(field, product);
        }

        // Умножение
        internal GaloisFieldPoly multiply(int scalar)
        {
            if (scalar == 0)
            {
                return field.Zero;
            }
            if (scalar == 1)
            {
                return this;
            }
            int size = coefficients.Length;
            int[] product = new int[size];
            for (int i = 0; i < size; i++)
            {
                product[i] = field.multiply(coefficients[i], scalar);
            }
            return new GaloisFieldPoly(field, product);
        }

        // Умножение на одночлен
        internal GaloisFieldPoly multiplyByMonomial(int degree, int coefficient)
        {
            if (degree < 0)
            {
                throw new ArgumentException();
            }
            if (coefficient == 0)
            {
                return field.Zero;
            }
            int size = coefficients.Length;
            int[] product = new int[size + degree];
            for (int i = 0; i < size; i++)
            {
                product[i] = field.multiply(coefficients[i], coefficient);
            }
            return new GaloisFieldPoly(field, product);
        }

        // Деление
        internal GaloisFieldPoly[] divide(GaloisFieldPoly other)
        {
            if (!field.Equals(other.field))
            {
                throw new ArgumentException("Полиномы должны относится к одному полю");
            }
            if (other.isZero)
            {
                throw new ArgumentException("Divide by 0");
            }

            GaloisFieldPoly quotient = field.Zero;
            GaloisFieldPoly remainder = this;

            int denominatorLeadingTerm = other.getCoefficient(other.Degree);
            int inverseDenominatorLeadingTerm = field.inverse(denominatorLeadingTerm);

            while (remainder.Degree >= other.Degree && !remainder.isZero)
            {
                int degreeDifference = remainder.Degree - other.Degree;
                int scale = field.multiply(remainder.getCoefficient(remainder.Degree), inverseDenominatorLeadingTerm);
                GaloisFieldPoly term = other.multiplyByMonomial(degreeDifference, scale);
                GaloisFieldPoly iterationQuotient = field.buildMonomial(degreeDifference, scale);
                quotient = quotient.addOrSubtract(iterationQuotient);
                remainder = remainder.addOrSubtract(term);
            }

            return new GaloisFieldPoly[] { quotient, remainder };
        }
    }

}
