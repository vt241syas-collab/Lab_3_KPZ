using System;

namespace GraphicEditorBridge
{
    public interface IRenderer
    {
        void RenderCircle(float radius);
        void RenderSquare(float side);
        void RenderTriangle(float sideA, float sideB);
    }

    public class VectorRenderer : IRenderer
    {
        public void RenderCircle(float radius) =>
            Console.WriteLine($"Малювання кола векторами з радіусом {radius}");

        public void RenderSquare(float side) =>
            Console.WriteLine($"Малювання квадрата векторами зі стороною {side}");

        public void RenderTriangle(float sideA, float sideB) =>
            Console.WriteLine($"Малювання трикутника векторами зі сторонами {sideA}, {sideB}");
    }

    public class RasterRenderer : IRenderer
    {
        public void RenderCircle(float radius) =>
            Console.WriteLine($"Малювання кола пікселями (растр) з радіусом {radius}");

        public void RenderSquare(float side) =>
            Console.WriteLine($"Малювання квадрата пікселями (растр) зі стороною {side}");

        public void RenderTriangle(float sideA, float sideB) =>
            Console.WriteLine($"Малювання трикутника пікселями (растр) зі сторонами {sideA}, {sideB}");
    }

    public abstract class Shape
    {
        protected IRenderer _renderer;

        protected Shape(IRenderer renderer)
        {
            _renderer = renderer;
        }

        public abstract void Draw();
    }

   public class Circle : Shape
    {
        private float _radius;
        public Circle(IRenderer renderer, float radius) : base(renderer) => _radius = radius;

        public override void Draw() => _renderer.RenderCircle(_radius);
    }

    public class Square : Shape
    {
        private float _side;
        public Square(IRenderer renderer, float side) : base(renderer) => _side = side;

        public override void Draw() => _renderer.RenderSquare(_side);
    }

    public class Triangle : Shape
    {
        private float _a, _b;
        public Triangle(IRenderer renderer, float a, float b) : base(renderer)
        {
            _a = a; _b = b;
        }

        public override void Draw() => _renderer.RenderTriangle(_a, _b);
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            IRenderer vector = new VectorRenderer();
            IRenderer raster = new RasterRenderer();

           Shape shape1 = new Circle(vector, 7.5f);
            Shape shape2 = new Square(raster, 12.0f);
            Shape shape3 = new Triangle(vector, 5.0f, 8.0f);

            Console.WriteLine("=== Робота графічного редактора (Bridge Pattern) ===\n");

            shape1.Draw();
            shape2.Draw();
            shape3.Draw();

            Console.WriteLine("\n--- Зміна рендерера для кола на растровий ---");
            Shape shape4 = new Circle(raster, 7.5f);
            shape4.Draw();

            Console.WriteLine("\nНатисніть будь-яку клавішу для завершення...");
            Console.ReadKey();
        }
    }
}