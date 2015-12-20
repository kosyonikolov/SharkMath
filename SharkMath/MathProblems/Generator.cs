﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharkMath.MathProblems.Problems;

namespace SharkMath.MathProblems
{
    public class Generator
    {
        public static Random random = new Random();

        /// <summary>
        /// Връша рационално число
        /// </summary>
        /// <param name="cd">описателя за числото. pIrational се игнорира</param>
        /// <returns></returns>
        public static Number getNumber(CoefDescriptor cd)
        {
            Number result = new Number();
            setNumber(result, cd);
            return result;
        }

        public static void setNumber(Number num, CoefDescriptor cd)
        {
            int typeRoll = random.Next(1, 101); // 1-100

            if (typeRoll <= cd.pNatural + cd.pIrrational) num.set(random.Next(cd.minNumerator, cd.maxNumerator + 1));
            else num.set(random.Next(cd.minNumerator, cd.maxNumerator + 1), random.Next(cd.minDenominator, cd.maxDenominator + 1));
            if (random.Next(100) >= 50) num.numerator *= -1;
        }

        /// <summary>
        /// Съставя полином с дадените буква, степен, и коефициенти
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="power"></param>
        /// <param name="cd">Описателя на коефициентите</param>
        /// <returns></returns>
        public static Polynomial getPolynomial(char letter, int power, CoefDescriptor cd)
        {
            Polynomial result = new Polynomial();
            for(int i = power; i >= 1; i--)
            {
                Monomial current = new Monomial(getNumber(cd), letter, (short)i);
                result.monos.Add(current);
            }
            result.monos.Add(new Monomial(getNumber(cd)));
            return result;
        }

        public static PolyNode getPolyNode(char letter, int power, CoefDescriptor cd)
        {
            PolyNode result = new PolyNode();
            for (int i = power; i >= 1; i--)
            {
                Monomial current = new Monomial(getNumber(cd), letter, (short)i);
                result.poly.monos.Add(current);
            }
            result.poly.monos.Add(new Monomial(getNumber(cd)));
            return result;
        }

        /// <summary>
        /// Намира дискриминанта на ax2 + bx + c = 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Number getD(Number a, Number b, Number c)
        {
            Number part = a * c;
            part.MultiplyBy(4);
            return b * b - part;
        }

        /// <summary>
        /// Създава такива a,b,c че дискриминантата да не е квадрат
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="cd"></param>
        public static void createNonSquareD(out Number a, out Number b, out Number c, CoefDescriptor cd)
        {
            a = new Number();
            b = new Number();
            c = new Number();

            while(true)
            {
                setNumber(a, cd);
                setNumber(b, cd);
                setNumber(c, cd);
                Number d = getD(a, b, c);
                if (!d.isNegative && d.isSquare() == false) return;
            }
        }

        /// <summary>
        /// Създава отрицателна дискриминанта
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="cd"></param>
        public static void createNegativeD(out Number a, out Number b, out Number c, CoefDescriptor cd)
        {
            a = new Number();
            b = new Number();
            c = new Number();

            while (true)
            {
                setNumber(a, cd);
                setNumber(b, cd);
                setNumber(c, cd);
                Number d = getD(a, b, c);
                if (d.isNegative) return;
            }
        }

        /// <summary>
        /// Намира ирационалните корени на ax2 + bc + c = 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Node[] getRoots(Number a, Number b, Number c)
        {
            a = new Number(a);
            b = new Number(b);
            c = new Number(c);

            Number d = getD(a, b, c); // намираме дискриминантата

            b.flipSign(); // правим -b

            PowerNode sqRoot = new PowerNode(new PolyNode(d), new Number(1, 2)); // правим корен от дискриминантата
            sqRoot.simplify(); // извеждаме коефициент

            a.MultiplyBy(2);
            Number g = Number.gcd(Number.gcd(sqRoot.coef, b), a); // търсим НОД на -b, sqrt(d) и 2*a

            if(!g.isOne)
            {
                a.DivideBy(g);
                b.DivideBy(g);
                sqRoot.coef.DivideBy(g);

                if(a.isOne) // съкращава се напълно
                {
                    Node[] result = new Node[2];
                    result[1] = new SumNode(new PolyNode(b), sqRoot);

                    sqRoot.flipSign();
                    result[0] = new SumNode(new PolyNode(b), sqRoot);
                    return result;
                }
            }

            PolyNode denom = new PolyNode(new Polynomial(a)); // не се е съкратило, трябва знаменател

            SumNode sum1 = new SumNode(new PolyNode(b), sqRoot); // -b + sqrt(d)
            sqRoot.flipSign();
            SumNode sum2 = new SumNode(new PolyNode(b), sqRoot); // -b - sqrt(d)

            Node[] _result = new Node[2];
            _result[1] = new FracNode(sum1, denom);
            _result[0] = new FracNode(sum2, denom);

            return _result;
        }

        /// <summary>
        /// Създава масив от цели числа със сума power и ги разпределя
        /// </summary>
        /// <param name="n">броя числа</param>
        /// <param name="power">сумарната степен</param>
        /// <returns></returns>
        private static int[] makePowerList(int n, int power)
        {
            int[] result = new int[n];
            result[0] = power;

            int[] chArr = new int[n];

            for (int i = 0; i < n - 1; i++)
            {
                int chArrLen = 0;
                for (int j = 0; j <= i; j++)
                { // на всяка стъпка записваме кои елементи имат степен над 1
                    if (result[j] > 1) chArr[chArrLen++] = j;
                }

                int choice = random.Next(chArrLen); // след това избираме 1 от тях

                int newInt = random.Next(1, result[chArr[choice]]);
                result[i + 1] = newInt; // и изваждаме от него някаква степен
                result[chArr[choice]] -= newInt; // за да я добавим на друго място
            } // сложност N^2 но за практически стойности няма смисъл да се пише по-сложно/оптимално
            return result;
        }

        public static Node getNode(char letter, int maxVisualPower, CoefDescriptor cd)
        {
            if (maxVisualPower < 2) throw new Exception("There is no point in maxVisualPower < 2!");
            int power = random.Next(maxVisualPower - 1) + 2;
            int nElements = random.Next(power) + 1;

            if(nElements == 1)
            {
                if(power == 1)
                {
                    return getPolyNode(letter, 1, cd);
                }
                else
                {
                    PowerNode pn = new PowerNode();
                    pn.powered = getPolyNode(letter, 1, cd);
                    pn.numPower.set(power);
                    return pn;
                }
            }
            else
            {
                int[] distrib = makePowerList(nElements, power);
                ProdNode result = new ProdNode();
                for(int i = 0; i < nElements; i++)
                {
                    int cPower = distrib[i];
                    if (cPower == 1) result.children.Add(getPolyNode(letter, 1, cd));
                    else
                    {
                        int typeRoll = random.Next(100) + 1;
                        if (typeRoll <= 50)
                        {
                            PowerNode pn = new PowerNode();
                            pn.powered = getPolyNode(letter, 1, cd);
                            pn.numPower.set(cPower);
                            result.children.Add(pn);
                        }
                        else result.children.Add(getPolyNode(letter, cPower, cd));
                    }
                }
                return result;
            }
        }

        public static SimpleEquation getEquation(char letter, SimpleEquationDescriptor sed)
        {
            SimpleEquation se = new SimpleEquation(letter);
            se.create(sed);

            int nTrans = random.Next(sed.minTransformations, sed.maxTransformations + 1);
            for(int i = 0; i < nTrans; i++)
            {
                Node current = getNode(letter, sed.maxVisualPower, sed.elemCoefDesc);
                bool choice = (random.Next()&1) == 0;
                se.left.addNode(current, choice);
                se.right.addNode(current, !choice);
            }

            //se.fixPolynomials();

            return se;
        }
    }
}
