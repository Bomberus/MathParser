# MathParser
A simple C# library to calculate a Math Equation

## How initialize it
Import the Parser.cs into your Project and add the namespace MathParser then initialize the MathParser

```
Parser parser = new Parser(null);
```

## How to use it
Get the result of the library
```
double result = parser.calculate("2*x^4"); //32
```

## Add Variables to the Parser
To use Variables in your expression create an List of the Type Variable and define your Variables
```
List<Variable> variable = new List<Variable>();
variable.Add(new Variable("x", 10));
Parser parser = new Parser(variable);
double result = parser.calculate("2*x"); //20
```
In this example the variable x has the value 10.

Be careful when using the Variable x because the function Integral uses it to define the functions x- value.

## List of supported functions
- sin(x)
- sqrt(x)
- log(x,y) = log{y}(x)
- abs(x) = |x|
- fac(x) = x!
- integral -> uses a numeric algorithm to get the result, but
be careful to use the right order of the parameters:

integral(f(x),min,max,count of iteration steps (the higher the better the result but uses more cpu time)

Example: integral(x^2,2,4,20) ~ 18,87
