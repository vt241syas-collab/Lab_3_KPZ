using System;
using System.Collections.Generic;
using System.Text;

namespace LightHtmlCompositeFlyweight
{
    public enum DisplayType { Block, Inline }
    public enum ClosingType { Normal, SelfClosing }

    public class ElementType
    {
        public string TagName { get; }
        public DisplayType Display { get; }
        public ClosingType Closing { get; }

        public ElementType(string tagName, DisplayType display, ClosingType closing)
        {
            TagName = tagName;
            Display = display;
            Closing = closing;
        }
    }

    public class FlyweightFactory
    {
        private readonly Dictionary<string, ElementType> _types = new Dictionary<string, ElementType>();

        public ElementType GetElementType(string tagName, DisplayType display, ClosingType closing)
        {
            string key = $"{tagName}_{display}_{closing}";
            if (!_types.ContainsKey(key))
            {
                _types[key] = new ElementType(tagName, display, closing);
            }
            return _types[key];
        }
    }

    public interface ILightNodeVisitor
    {
        void Visit(LightTextNode node);
        void Visit(LightElementNode node);
    }

    public class TextExtractionVisitor : ILightNodeVisitor
    {
        private StringBuilder _extractedText = new StringBuilder();
        public void Visit(LightTextNode node) => _extractedText.AppendLine(node.InnerHTML());
        public void Visit(LightElementNode node) { }
        public string GetExtractedText() => _extractedText.ToString();
    }

    //ПАТЕРН КОМАНДА
    
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    
    public class AddNodeCommand : ICommand
    {
        private readonly LightElementNode _parent;
        private readonly LightNode _child;

        public AddNodeCommand(LightElementNode parent, LightNode child)
        {
            _parent = parent;
            _child = child;
        }

        public void Execute()
        {
            _parent.AddChild(_child);
            Console.WriteLine($"[Command] Виконано: додано вузол до <{_parent.TagName}>");
        }

        public void Undo()
        {
            _parent.RemoveChild(_child);
            Console.WriteLine($"[Command] Скасовано: видалено вузол з <{_parent.TagName}>");
        }
    }
   
    public abstract class LightNode
    {
        public abstract string OuterHTML();
        public abstract string InnerHTML();
        public abstract IEnumerable<LightNode> TraverseDFS();
        public abstract void Accept(ILightNodeVisitor visitor);
    }

    public class LightTextNode : LightNode
    {
        private readonly string _text;
        public LightTextNode(string text) => _text = text;

        public override string OuterHTML() => _text;
        public override string InnerHTML() => _text;
        public override IEnumerable<LightNode> TraverseDFS() { yield return this; }
        public override void Accept(ILightNodeVisitor visitor) => visitor.Visit(this);
    }

    public class LightElementNode : LightNode
    {
        private readonly ElementType _type;
        private readonly List<LightNode> _children = new List<LightNode>();
        public List<string> CssClasses { get; } = new List<string>();

       public string TagName => _type.TagName;

        public LightElementNode(ElementType type) => _type = type;

        public void AddChild(LightNode node) => _children.Add(node);

       public void RemoveChild(LightNode node) => _children.Remove(node);

        public int ChildrenCount => _children.Count;

        public override IEnumerable<LightNode> TraverseDFS()
        {
            yield return this;
            foreach (var child in _children)
            {
                foreach (var node in child.TraverseDFS()) yield return node;
            }
        }

        public override void Accept(ILightNodeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in _children) child.Accept(visitor);
        }

        public override string InnerHTML()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var child in _children) sb.Append(child.OuterHTML());
            return sb.ToString();
        }

        public override string OuterHTML()
        {
            StringBuilder sb = new StringBuilder();
            string classes = CssClasses.Count > 0 ? $" class=\"{string.Join(" ", CssClasses)}\"" : "";
            sb.Append($"<{_type.TagName}{classes}");

            if (_type.Closing == ClosingType.SelfClosing) sb.Append(" />");
            else
            {
                sb.Append(">");
                sb.Append(InnerHTML());
                sb.Append($"</{_type.TagName}>");
            }

            return _type.Display == DisplayType.Block ? sb.ToString() + Environment.NewLine : sb.ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            FlyweightFactory factory = new FlyweightFactory();

            string bookText = "ACT V\n" +
               "Scene I. Mantua.\n" +
               "Scene II. Friar Lawrence.\n" +
               "Scene III. A churchyard; in it a Monument belonging to the Capulets." +
               " Dramatis Personæ" +
               "ESCALUS, Prince of Verona.\r\nMERCUTIO, kinsman to the Prince, and friend to Romeo.\r\nPARIS, a young Nobleman, kinsman to the Prince.\r\nPage to Paris.";

            string[] lines = bookText.Split('\n');
            var bodyType = factory.GetElementType("body", DisplayType.Block, ClosingType.Normal);
            var body = new LightElementNode(bodyType);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                ElementType type;
                if (i == 0) type = factory.GetElementType("h1", DisplayType.Block, ClosingType.Normal);
                else if (line.Trim().Length < 20) type = factory.GetElementType("h2", DisplayType.Block, ClosingType.Normal);
                else if (char.IsWhiteSpace(line[0])) type = factory.GetElementType("blockquote", DisplayType.Block, ClosingType.Normal);
                else type = factory.GetElementType("p", DisplayType.Block, ClosingType.Normal);

                var element = new LightElementNode(type);
                element.AddChild(new LightTextNode(line.Trim()));
                body.AddChild(element);
            }

            //  ТЕСТ ПАТЕРНА КОМАНДА 
          
            Console.WriteLine("\n=== ТЕСТ КОМАНДИ (Додавання та Скасування) ===");

            var footerType = factory.GetElementType("footer", DisplayType.Block, ClosingType.Normal);
            var footerElement = new LightElementNode(footerType);
            footerElement.AddChild(new LightTextNode("The end of the book."));

           ICommand addFooterCommand = new AddNodeCommand(body, footerElement);

           addFooterCommand.Execute();
            Console.WriteLine($"Дітей у body ПІСЛЯ Execute: {body.ChildrenCount}");

           
            addFooterCommand.Undo();
            Console.WriteLine($"Дітей у body ПІСЛЯ Undo: {body.ChildrenCount}");
            


            Console.WriteLine("\nВЕРСТКА СТОРІНКИ\n");
            Console.WriteLine(body.OuterHTML());

            Console.WriteLine("\nТЕСТ ІТЕРАТОРА");
            int nodeCount = 0;
            foreach (var node in body.TraverseDFS()) nodeCount++;
            Console.WriteLine($"Загальна кількість вузлів у дереві: {nodeCount}");

            Console.WriteLine("\nТЕСТ ВІДВІДУВАЧА");
            var textVisitor = new TextExtractionVisitor();
            body.Accept(textVisitor);
            Console.WriteLine("Текст успішно витягнуто (символів: " + textVisitor.GetExtractedText().Length + ")");

            Console.ReadKey();
        }
    }
}