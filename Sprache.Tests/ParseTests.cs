using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sprache.Tests
{
    [TestFixture]
    public class ParseTests
    {
        [Test]
        public void Parser_OfChar_AcceptsThatChar()
        {
            AssertParser.SucceedsWithOne(Parse.Char('a').Once(), "a", 'a');
        }

        [Test]
        public void Parser_OfChar_AcceptsOnlyOneChar()
        {
            AssertParser.SucceedsWithOne(Parse.Char('a').Once(), "aaa", 'a');
        }

        [Test]
        public void Parser_OfChar_DoesNotAcceptNonMatchingChar()
        {
            AssertParser.FailsAt(Parse.Char('a').Once(), "b", 0);
        }

        [Test]
        public void Parser_OfChar_DoesNotAcceptEmptyInput()
        {
            AssertParser.Fails(Parse.Char('a').Once(), "");
        }

        [Test]
        public void Parser_OfManyChars_AcceptsEmptyInput()
        {
            AssertParser.SucceedsWithAll(Parse.Char('a').Many(), "");
        }

        [Test]
        public void Parser_OfManyChars_AcceptsManyChars()
        {
            AssertParser.SucceedsWithAll(Parse.Char('a').Many(), "aaa");
        }

        [Test]
        public void Parser_OfAtLeastOneChar_DoesNotAcceptEmptyInput()
        {
            AssertParser.Fails(Parse.Char('a').AtLeastOnce(), "");
        }

        [Test]
        public void Parser_OfAtLeastOneChar_AcceptsOneChar()
        {
            AssertParser.SucceedsWithAll(Parse.Char('a').AtLeastOnce(), "a");
        }

        [Test]
        public void Parser_OfAtLeastOneChar_AcceptsManyChars()
        {
            AssertParser.SucceedsWithAll(Parse.Char('a').AtLeastOnce(), "aaa");
        }

        [Test]
        public void ConcatenatingParsers_ConcatenatesResults()
        {
            var p = Parse.Char('a').Once().Then(a =>
                Parse.Char('b').Once().Select(b => a.Concat(b)));
            AssertParser.SucceedsWithAll(p, "ab"); 
        }

        [Test]
        public void ReturningValue_DoesNotAdvanceInput()
        {
            var p = Parse.Return(1);
            AssertParser.SucceedsWith(p, "abc", n => Assert.AreEqual(1, n));
        }

        [Test]
        public void ReturningValue_ReturnsValueAsResult()
        {
            var p = Parse.Return(1);
            var r = (Success<int>)p.Parse("abc");
            Assert.AreEqual(0, r.Remainder.Position);
        }

        [Test]
        public void CanSpecifyParsersUsingQueryComprehensions()
        {
            var p = from a in Parse.Char('a').Once()
                    from bs in Parse.Char('b').Many()
                    from cs in Parse.Char('c').AtLeastOnce()
                    select a.Concat(bs).Concat(cs);

            AssertParser.SucceedsWithAll(p, "abbbc");
        }

        [Test]
        public void WhenFirstOptionSucceedsButConsumesNothing_SecondOptionTried()
        {
            var p = Parse.Char('a').Many().Or(Parse.Char('b').Many());
            AssertParser.SucceedsWithAll(p, "bbb");
        }

        [Test]
        public void WhenFirstOptionFailsAndConsumesInput_SecondOptionNotTried()
        {
            var first = Parse.Char('a').Once().Concat(Parse.Char('b').Once());
            var second = Parse.Char('a').Once();
            var p = first.Or(second);
            AssertParser.FailsAt(p, "a", 1);
        }

        [Test]
        public void WithTry_WhenFirstOptionFailsAndConsumesInput_SecondOptionTried()
        {
            var first = Parse.Char('a').Once().Concat(Parse.Char('b').Once());
            var second = Parse.Char('a').Once();
            var p = first.Try().Or(second);
            AssertParser.SucceedsWithAll(p, "a");
        }

        [Test]
        public void ParsesString_AsSequenceOfChars()
        {
            var p = Parse.String("abc");
            AssertParser.SucceedsWithAll(p, "abc");
        }

        static readonly Parser<IEnumerable<char>> ASeq =
            (from first in Parse.Ref(() => ASeq)
             from comma in Parse.Char(',')
             from rest in Parse.Char('a').Once()
             select first.Concat(rest))
            .Or(Parse.Char('a').Once());

        [Test]
        public void CanParseLeftRecursiveGrammar()
        {
            AssertParser.SucceedsWith(ASeq, "a,a,a", r => new string(r.ToArray()).Equals("aaa"));
        }
    }
}
