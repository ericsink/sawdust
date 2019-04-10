
#if DEBUG

using System;
using System.Drawing;

using NUnit.Framework;

namespace sd
{
    [TestFixture]
    public class test_expr
    {
        [Test]
        public void test_unary_minus()
        {
            Plan p = new Plan("none");
            p.DefineVariable(new VariableDefinition("a", "TODO help", 4, 6, 5, 1));

            Assert.IsTrue(fp.eq_unknowndata(-16, Expr.Evaluate("4 * -4", p)));
            Assert.IsTrue(fp.eq_unknowndata(4, Expr.Evaluate("8 + -4", p)));
            Assert.IsTrue(fp.eq_unknowndata(16, Expr.Evaluate("-4 * -4", p)));
            Assert.IsTrue(fp.eq_unknowndata(0, Expr.Evaluate("-4 - -4", p)));
            Assert.IsTrue(fp.eq_unknowndata(-20, Expr.Evaluate("4 * -a", p)));
            Assert.IsTrue(fp.eq_unknowndata(-5, Expr.Evaluate("-a", p)));
        }

        public void test_ParseDouble_one_valid(double d)
        {
            Assert.AreEqual(ut.ParseDouble(d.ToString(), -45), d);
        }

        public void test_ParseDouble_one_invalid(string s)
        {
            Assert.AreEqual(ut.ParseDouble(s, 42), 42);
        }

        [Test]
        public void test_ParseDouble()
        {
            // valid cases
            test_ParseDouble_one_valid(3);
            test_ParseDouble_one_valid(0);
            test_ParseDouble_one_valid(1.414);
            test_ParseDouble_one_valid(3.14);
            test_ParseDouble_one_valid(-45);
            test_ParseDouble_one_valid(-22);
            test_ParseDouble_one_valid(-3.14159);

            // invalid cases
            test_ParseDouble_one_invalid("plok");
            test_ParseDouble_one_invalid("1.2.3.4.5.6");
            test_ParseDouble_one_invalid("1. 414");
            test_ParseDouble_one_invalid("3.1 4");
            test_ParseDouble_one_invalid("-45-22");
            test_ParseDouble_one_invalid("-9999w999");
            test_ParseDouble_one_invalid("500 600 700");
        }

        [Test]
        public void test_expr_2()
        {
            Plan p = new Plan("none");
            p.DefineVariable(new VariableDefinition("a", "TODO help", 0, 99, 5, 64));
            p.DefineVariable(new VariableDefinition("b", "TODO help", 0, 99, 7, 64));
            p.DefineVariable(new VariableDefinition("c", "TODO help", 0, 99, 2, 64));
            p.DefineVariable(new VariableDefinition("d", "TODO help", 0, 99, 6, 64));

            bool bfailed = false;
            try
            {
                Expr.Evaluate("45 * 7)", p);
            }
            catch
            {
                bfailed = true;
            }
            Assert.IsTrue(bfailed);

            bfailed = false;
            try
            {
                Expr.Evaluate("45 87", p);
            }
            catch
            {
                bfailed = true;
            }
            Assert.IsTrue(bfailed);

            bfailed = false;
            try
            {
                Expr.Evaluate("45 * 6%", p);
            }
            catch
            {
                bfailed = true;
            }
            Assert.IsTrue(bfailed);

            bfailed = false;
            try
            {
                Expr.Evaluate("45 * 6$", p);
            }
            catch
            {
                bfailed = true;
            }
            Assert.IsTrue(bfailed);

            bfailed = false;
            try
            {
                Expr.Evaluate("(45 * 6", p);
            }
            catch
            {
                bfailed = true;
            }
            Assert.IsTrue(bfailed);
        }

        [Test]
        public void text_expr_1()
        {
            Plan p = new Plan("none");
            p.DefineVariable(new VariableDefinition("a", "TODO help", 0, 99, 5, 1));
            p.DefineVariable(new VariableDefinition("b", "TODO help", 0, 99, 7, 1));
            p.DefineVariable(new VariableDefinition("c", "TODO help", 0, 99, 2, 1));
            p.DefineVariable(new VariableDefinition("d", "TODO help", 0, 99, 6, 1));

            Assert.IsTrue(fp.eq_unknowndata(0, Expr.Evaluate("4 - 4", p)));
            Assert.IsTrue(fp.eq_unknowndata(0, Expr.Evaluate("4 - 8 + 4", p)));
            Assert.IsTrue(fp.eq_unknowndata(0, Expr.Evaluate("4 - 2 - 2", p)));
            Assert.IsTrue(fp.eq_unknowndata(2, Expr.Evaluate("(3+5)/(2+2)", p)));
            Assert.IsTrue(fp.eq_unknowndata(2, Expr.Evaluate("b - a", p)));

            bool bfailed = false;
            try
            {
                Expr.Evaluate("57 * q", p);
            }
            catch
            {
                bfailed = true;
            }
            Assert.IsTrue(bfailed);
        }
    }
}

#endif
