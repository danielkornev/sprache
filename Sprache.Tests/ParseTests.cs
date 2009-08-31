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
            AssertParser.SucceedsWithOne(Parse.Char('a'), "a", 'a');
        }

        [Test]
        public void Parser_OfChar_AcceptsOnlyOneChar()
        {
            AssertParser.SucceedsWithOne(Parse.Char('a'), "aaa", 'a');
        }

        [Test]
        public void Parser_OfChar_DoesNotAcceptNonMatchingChar()
        {
            AssertParser.FailsAt(Parse.Char('a'), "b", 0);
        }

        [Test]
        public void Parser_OfChar_DoesNotAcceptEmptyInput()
        {
            AssertParser.Fails(Parse.Char('a'), "");
        }

        [Test]
        public void Parser_OfManyChars_AcceptsEmptyInput()
        {
            AssertParser.SucceedsWithAll(Parse.Char('a').Repeat(), "");
        }

        [Test]
        public void Parser_OfManyChars_AcceptsManyChars()
        {
            AssertParser.SucceedsWithAll(Parse.Char('a').Repeat(), "aaa");
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
            var p = Parse.Char('a').Concat(Parse.Char('b'));
            AssertParser.SucceedsWithAll(p, "ab"); 
        }
    }
}
