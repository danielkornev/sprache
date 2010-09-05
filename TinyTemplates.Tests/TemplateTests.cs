using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TinyTemplates.Tests
{
    [TestFixture]
    public class TemplateTests
    {
        static readonly DateTime SaturdayInSeptember = new DateTime(2010, 9, 4);

        [Test]
        public void AnEmptyTemplateProducesNoOutput()
        {
            var tt = new Template("");
            var o = tt.Execute(new object());
            Assert.AreEqual("", o);
        }

        [Test]
        public void LiteralTextIsOutputLiterally()
        {
            const string txt = "abc";
            var tt = new Template(txt);
            var o = tt.Execute(new object());
            Assert.AreEqual(txt, o);
        }

        [Test]
        public void ADoubleHashIsEscaped()
        {
            var tt = new Template("##");
            var o = tt.Execute(new object());
            Assert.AreEqual("#", o);
        }

        [Test]
        public void AHashBeforeAnIdentifierSubstitutesAModelProperty()
        {
            var tt = new Template("#DayOfWeek");
            var o = tt.Execute(SaturdayInSeptember);
            Assert.AreEqual(DayOfWeek.Saturday.ToString(), o);
        }

        [Test]
        public void BracesOptionallyDelimitDirectives()
        {
            var tt = new Template("#{DayOfWeek}");
            var o = tt.Execute(SaturdayInSeptember);
            Assert.AreEqual(DayOfWeek.Saturday.ToString(), o);
        }

        [Test]
        public void IdentifiersAreNotCaseSensitive()
        {
            var tt = new Template("#DAYOFWEEK");
            var o = tt.Execute(SaturdayInSeptember);
            Assert.AreEqual(DayOfWeek.Saturday.ToString(), o);
        }
    }
}
