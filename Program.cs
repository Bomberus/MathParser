using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathParser
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Variable> variable = new List<Variable>();
            variable.Add(new Variable("x", 10));
            Parser parser = new Parser(variable);
            Console.WriteLine(parser.calculate("x"));
            Console.WriteLine("Write your Equation:");
            Console.WriteLine(parser.calculate(Console.ReadLine()));
            Console.WriteLine("Exit with any Key");
            Console.ReadLine();
        }
    }
}
