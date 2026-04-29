using System;
using System.Collections.Generic;
using System.Linq;
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

    // ==========================================================
    // === ПАТЕРН ВІДВІДУВАЧ (Visitor) - ІНТЕРФЕЙСИ ТА КЛАСИ ===
    // ==========================================================
    public interface ILightNodeVisitor
    {
        void Visit(LightTextNode node);
        void Visit(LightElementNode node);
    }

    public class TextExtractionVisitor : ILightNodeVisitor
    {
        private StringBuilder _extractedText = new StringBuilder();

        public void Visit(LightTextNode node)
        {
            _extractedText.AppendLine(node.InnerHTML());
        }

        public void Visit(LightElementNode node)
        {
            // Елементи (теги) ми ігноруємо, бо нам потрібен тільки текст
        }

        public string GetExtractedText() => _extractedText.ToString();
    }
    // ==========================================================

    public abstract class LightNode
    {
        public abstract string OuterHTML();
        public abstract string InnerHTML();
        public abstract IEnumerable<LightNode> TraverseDFS();

        // Додано для патерна Відвідувач
        public abstract void Accept(ILightNodeVisitor visitor);
    }

    public class LightTextNode : LightNode
    {
        private readonly string _text;
        public LightTextNode(string text) => _text = text;

        public override string OuterHTML() => _text;
        public override string InnerHTML() => _text;

        public override IEnumerable<LightNode> TraverseDFS()
        {
            yield return this;
        }

        // Додано для патерна Відвідувач
        public override void Accept(ILightNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LightElementNode : LightNode
    {
        private readonly ElementType _type;
        private readonly List<LightNode> _children = new List<LightNode>();
        public List<string> CssClasses { get; } = new List<string>();

        public LightElementNode(ElementType type) => _type = type;

        public void AddChild(LightNode node) => _children.Add(node);

        public int ChildrenCount => _children.Count;

        public override IEnumerable<LightNode> TraverseDFS()
        {
            yield return this;
            foreach (var child in _children)
            {
                foreach (var node in child.TraverseDFS())
                {
                    yield return node;
                }
            }
        }

        // Додано для патерна Відвідувач
        public override void Accept(ILightNodeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in _children)
            {
                child.Accept(visitor);
            }
        }

        public override string InnerHTML()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var child in _children)
                sb.Append(child.OuterHTML());
            return sb.ToString();
        }

        public override string OuterHTML()
        {
            StringBuilder sb = new StringBuilder();
            string classes = CssClasses.Count > 0 ? $" class=\"{string.Join(" ", CssClasses)}\"" : "";

            sb.Append($"<{_type.TagName}{classes}");

            if (_type.Closing == ClosingType.SelfClosing)
            {
                sb.Append(" />");
            }
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

            long memStart = GC.GetTotalMemory(true);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                ElementType type;
                if (i == 0)
                    type = factory.GetElementType("h1", DisplayType.Block, ClosingType.Normal);
                else if (line.Trim().Length < 20)
                    type = factory.GetElementType("h2", DisplayType.Block, ClosingType.Normal);
                else if (char.IsWhiteSpace(line[0]))
                    type = factory.GetElementType("blockquote", DisplayType.Block, ClosingType.Normal);
                else
                    type = factory.GetElementType("p", DisplayType.Block, ClosingType.Normal);

                var element = new LightElementNode(type);
                element.AddChild(new LightTextNode(line.Trim()));
                body.AddChild(element);
            }

            long memEnd = GC.GetTotalMemory(true);

            Console.WriteLine("=== ВЕРСТКА СТОРІНКИ (Composite + Flyweight) ===\n");
            Console.WriteLine(body.OuterHTML());

            Console.WriteLine("\n=== СТАТИСТИКА ===");
            Console.WriteLine($"Кількість елементів у body: {body.ChildrenCount}");
            Console.WriteLine($"Використано пам'яті: {memEnd - memStart} байт");

            // === ТЕСТ ІТЕРАТОРА ===
            Console.WriteLine("\nТЕСТ ІТЕРАТОРА");
            int nodeCount = 0;
            foreach (var node in body.TraverseDFS())
            {
                nodeCount++;
                if (node is LightElementNode element)
                {
                    Console.WriteLine($"Вузол {nodeCount}: Елемент (Кількість дітей: {element.ChildrenCount})");
                }
                else if (node is LightTextNode textNode)
                {
                    Console.WriteLine($"Вузол {nodeCount}: Текст -> {textNode.InnerHTML()}");
                }
            }
            Console.WriteLine($"Загальна кількість вузлів у дереві: {nodeCount}");

            // ТЕСТ ВІДВІДУВАЧА 
            Console.WriteLine("\nТЕСТ ВІДВІДУВАЧА ");
            var textVisitor = new TextExtractionVisitor();

            body.Accept(textVisitor);

            Console.WriteLine(textVisitor.GetExtractedText());

            Console.ReadKey();
        }
    }
}