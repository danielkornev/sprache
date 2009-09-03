using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sprache;

namespace XmlExample
{
    public class Document
    {
        public Node Root;
    }

    public class Item { }

    public class Content : Item
    {
        public string Text;
    }

    public class Node : Item
    {
        public string Name;
        public IEnumerable<Item> Children;
    }

    public class BeginTag
    {
        public string Name;
    }

    public static class XmlParser
    {
        static readonly Parser<string> Identifier =
            from first in Parse.Letter.Once()
            from rest in Parse.LetterOrDigit.Or(Parse.Char('-')).Or(Parse.Char('_')).Many()
            select new string(first.Concat(rest).ToArray());

        static Parser<T> Tag<T>(Parser<T> content)
        {
            return from lead in Parse.WhiteSpace.Many()
                   from lt in Parse.Char('<')
                   from t in content
                   from gt in Parse.Char('>')
                   from trail in Parse.WhiteSpace.Many()
                   select t;
        }

        static readonly Parser<BeginTag> BeginTag= 
            Tag(from id in Identifier
                select new BeginTag { Name = id });

        static Parser<string> EndTag(string name)
        {
            return Tag(from slash in Parse.Char('/')
                       from id in Identifier
                       where id == name
                       select id);
        }

        static readonly Parser<Content> Content =
            from chars in Parse.Char(c => c != '<', "content").Many()
            select new Content { Text = new string(chars.ToArray()) };

        static readonly Parser<Node> FullNode =
            from tag in BeginTag
            from nodes in Parse.Ref(() => Item).Many()
            from end in EndTag(tag.Name)
            select new Node { Name = tag.Name, Children = nodes };

        static readonly Parser<Node> ShortNode = Tag(from id in Identifier
                                                     from slash in Parse.Char('/')
                                                     select new Node { Name = id });
        
        static readonly Parser<Node> Node = ShortNode.Try().Or(FullNode);

        static readonly Parser<Item> Item = Node.Select(n => (Item)n).Or(Content.Select(c => (Item)c));

        public static readonly Parser<Document> Document =
            Node.Select(n => new Document { Root = n });
    }

    class Program
    {
        static void Main(string[] args)
        {
            var doc = "<body><p>hello,<br/> <i>world!</i></p></body>";
            var parsed = XmlParser.Document.Parse(doc);
            Console.WriteLine(parsed);
        }
    }
}
