using System;

namespace RpgGame
{  public interface IHero
    {
        string GetDescription();
        double GetAttack();
    }

    public class Warrior : IHero
    {
        public string GetDescription() => "Воїн";
        public double GetAttack() => 15.0;
    }

    public class Mage : IHero
    {
        public string GetDescription() => "Маг";
        public double GetAttack() => 25.0;
    }

    public class Paladin : IHero
    {
        public string GetDescription() => "Паладин";
        public double GetAttack() => 20.0;
    }

    public abstract class InventoryDecorator : IHero
    {
        protected IHero _hero;

        public InventoryDecorator(IHero hero)
        {
            _hero = hero;
        }

        public virtual string GetDescription() => _hero.GetDescription();
        public virtual double GetAttack() => _hero.GetAttack();
    }

  
    // Зброя 
    public class WeaponDecorator : InventoryDecorator
    {
        public WeaponDecorator(IHero hero) : base(hero) { }

        public override string GetDescription() => _hero.GetDescription() + " + Сталевий меч";
        public override double GetAttack() => _hero.GetAttack() + 10.0;
    }

    // Одяг 
    public class ArmorDecorator : InventoryDecorator
    {
        public ArmorDecorator(IHero hero) : base(hero) { }

        public override string GetDescription() => _hero.GetDescription() + " + Важка броня";
        public override double GetAttack() => _hero.GetAttack() + 3.5;
    }

    // Артефакт 
    public class ArtifactDecorator : InventoryDecorator
    {
        public ArtifactDecorator(IHero hero) : base(hero) { }

        public override string GetDescription() => _hero.GetDescription() + " + Магічне кільце";
       public override double GetAttack() => _hero.GetAttack() * 1.7;
    }

   
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("--- Створення героїв з інвентарем ---\n");

            
            IHero warrior = new Warrior();
            warrior = new ArmorDecorator(warrior);
            warrior = new WeaponDecorator(warrior);
            warrior = new WeaponDecorator(warrior); //

            PrintHeroInfo(warrior);

           
            IHero mage = new Mage();
            mage = new ArtifactDecorator(mage);

            PrintHeroInfo(mage);

            
            IHero paladin = new Paladin();
            paladin = new WeaponDecorator(paladin);
            paladin = new ArmorDecorator(paladin);
            paladin = new ArtifactDecorator(paladin);

            PrintHeroInfo(paladin);

            Console.WriteLine("Натисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }

        static void PrintHeroInfo(IHero hero)
        {
            Console.WriteLine($"Персонаж: {hero.GetDescription()}");
            Console.WriteLine($"Підсумкова атака: {hero.GetAttack():F1}");
            Console.WriteLine(new string('-', 50));
        }
    }
}