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

    //ПАТЕРН ВІДВІДУВАЧ
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

    //ПАТЕРН СТЕЙТ
    public interface INodeState
    {
        string Render(LightNode node, Func<string> defaultRender);
    }

    public class VisibleState : INodeState
    {
        public string Render(LightNode node, Func<string> defaultRender) => defaultRender();
    }

    public class HiddenState : INodeState
    {
        public string Render(LightNode node, Func<string> defaultRender) => ""; 
    }

    public abstract class LightNode
    {
        //ПАТЕРН ШАБЛОННИЙ МЕТОД
        public string Render()
        {
            return GetOpeningTag() + GetContent() + GetClosingTag();
        }

        protected abstract string GetOpeningTag();
        protected abstract string GetContent();
        protected abstract string GetClosingTag();

        public abstract string OuterHTML();
        public abstract string InnerHTML();
        public abstract IEnumerable<LightNode> TraverseDFS(); //ПАТЕРН ІТЕРАТОР 
        public abstract void Accept(ILightNodeVisitor visitor);
    }

    public class LightTextNode : LightNode
    {
        private readonly string _text;
        public LightTextNode(string text) => _text = text;

        public override string OuterHTML() => Render();
        public override string InnerHTML() => _text;

        protected override string GetOpeningTag() => "";
        protected override string GetContent() => _text;
        protected override string GetClosingTag() => "";

        public override IEnumerable<LightNode> TraverseDFS() { yield return this; }
        public override void Accept(ILightNodeVisitor visitor) => visitor.Visit(this);
    }

    public class LightElementNode : LightNode
    {
        private readonly ElementType _type;
        private readonly List<LightNode> _children = new List<LightNode>();
        public List<string> CssClasses { get; } = new List<string>();

      
        public INodeState State { get; set; } = new VisibleState();

        public string TagName => _type.TagName;
        public LightElementNode(ElementType type) => _type = type;

        public void AddChild(LightNode node) => _children.Add(node);
        public void RemoveChild(LightNode node) => _children.Remove(node);
        public int ChildrenCount => _children.Count;

    
        public override string OuterHTML() => State.Render(this, () => Render());

        public override string InnerHTML()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var child in _children) sb.Append(child.OuterHTML());
            return sb.ToString();
        }

        protected override string GetOpeningTag()
        {
            string classes = CssClasses.Count > 0 ? $" class=\"{string.Join(" ", CssClasses)}\"" : "";
            return $"<{_type.TagName}{classes}" + (_type.Closing == ClosingType.SelfClosing ? " />" : ">");
        }

        protected override string GetContent()
        {
            return _type.Closing == ClosingType.SelfClosing ? "" : InnerHTML();
        }

        protected override string GetClosingTag()
        {
            if (_type.Closing == ClosingType.SelfClosing) return _type.Display == DisplayType.Block ? Environment.NewLine : "";
            string close = $"</{_type.TagName}>";
            return _type.Display == DisplayType.Block ? close + Environment.NewLine : close;
        }

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

            Console.WriteLine("\nВЕРСТКА СТОРІНКИ\n");
            Console.WriteLine(body.OuterHTML());

            // ТЕСТ ПАТЕРНА СТЕЙТ 
            Console.WriteLine("\nТЕСТ СТЕЙТУ");
            Console.WriteLine("Змінюємо стан body на Hidden. HTML вивід має зникнути:");
            body.State = new HiddenState();
            Console.WriteLine($"Результат: '{body.OuterHTML()}'");

            Console.WriteLine("Повертаємо стан body на Visible.");
            body.State = new VisibleState();

            // ТЕСТ ПАТЕРНА КОМАНДА 
            Console.WriteLine("\nТЕСТ КОМАНДИ");
            var footerType = factory.GetElementType("footer", DisplayType.Block, ClosingType.Normal);
            var footerElement = new LightElementNode(footerType);
            footerElement.AddChild(new LightTextNode("The end of the book."));

            ICommand addFooterCommand = new AddNodeCommand(body, footerElement);
            addFooterCommand.Execute();
            Console.WriteLine($"Дітей у body ПІСЛЯ Execute: {body.ChildrenCount}");
            addFooterCommand.Undo();
            Console.WriteLine($"Дітей у body ПІСЛЯ Undo: {body.ChildrenCount}");

            // ТЕСТ ІТЕРАТОРА
            Console.WriteLine("\nТЕСТ ІТЕРАТОРА");
            int nodeCount = 0;
            foreach (var node in body.TraverseDFS()) nodeCount++;
            Console.WriteLine($"Загальна кількість вузлів у дереві: {nodeCount}");

            // ТЕСТ ВІДВІДУВАЧА
            Console.WriteLine("\nТЕСТ ВІДВІДУВАЧА");
            var textVisitor = new TextExtractionVisitor();
            body.Accept(textVisitor);
            Console.WriteLine("Текст успішно витягнуто (символів: " + textVisitor.GetExtractedText().Length + ")");

            Console.ReadKey();
        }
    }
}