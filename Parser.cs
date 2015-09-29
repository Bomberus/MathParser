using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MathParser
{
    enum TType { number, operation, function };

    #region Token
    class Token
    {
        public String name;
        public TType type;

        public Token(String iname, TType iType)
        {
            name = iname;
            type = iType;
        }
    }
    #endregion

    #region Variable
    public class Variable
    {
        public String name;
        public double wert;


        public Variable(String iname, double iwert)
        {
            name = iname;
            wert = iwert;
        }
    }
    #endregion

    #region Functioncalc
    class Function
    {
        public List<double> parameters;
        public string term = "";
        public String type;

        public Function(string itype)
        {
            parameters = new List<double>();
            type = itype;
        }

        public double calculate()
        {
            double ret = 0;
            switch (type)
            {
                case "(":
                    ret = parameters[0];
                    break;

                case "sin(":
                    ret = Math.Sin(parameters[0]);
                    break;

                case "sqrt(":
                    if (parameters.Count == 1)
                        ret = Math.Sqrt(parameters[0]);
                    else
                        ret = Math.Pow(parameters[1], 1 / parameters[0]);
                    break;
                case "log(":
                    if (parameters.Count == 1)
                        ret = Math.Log(parameters[0]);
                    else
                        ret = Math.Log(parameters[1], parameters[0]);
                    break;

                case "abs(":
                    ret = Math.Abs(parameters[0]);
                    break;
                case "fac(":
                    ret = parameters[0];
                    int tempret = 1;
                    if (ret == (int)ret)
                    {
                        for (int ii = 2; ii <= (int)ret; ii++)
                            tempret *= ii;
                        ret = tempret;
                    }
                    else
                    {
                        ret = (Math.Sqrt(3 * Math.PI * (6 * ret + 1)) * Math.Pow(ret, ret) * Math.Pow(Math.E, -ret)) / 3;
                    }

                    break;
                case "integral(":
                    double min = parameters[0];
                    double max = parameters[1];                   
                    int n = (int)parameters[2];
                    double h = 0;
                    string trapezregel = "";
                    Parser parser = new Parser(null);

                    if (n > 0)
                    {
                        ret = 0;
                        h = (max - min) / n;
                        //(f(a) + f(b)) / 2
                        trapezregel = "(" + term.Replace("x", min.ToString(CultureInfo.CreateSpecificCulture("en-GB"))) +"+"+
                            term.Replace("x", max.ToString(CultureInfo.CreateSpecificCulture("en-GB"))) + ")/2";

                        ret += parser.calculate(trapezregel) ;

                        for (int ii = 1; ii < n; ii++)
                        {
                            //<-- + f(a+ i*h)
                            trapezregel = term.Replace("x", "(" + min.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "+" + ii.ToString() +
                                "*" + h.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + ")");
                            ret += parser.calculate(trapezregel); 
                        }
                        ret *= h;
                    }
                    else
                        ret = 0;
                    
                    break;
            }
            return Math.Round(ret,4);
        }
    }
    #endregion

    public class Parser
    {
        private char[] operators = { '+', '*', '-', '/', '^', '%' };
        private char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };
        private string[] availableFunc = { "sin(", "(", "abs(", "sqrt(", "log(", "fac(", "integral(" };

        private List<Variable> variables;

        public Parser(List<Variable> ivar)
        {
            variables = ivar;
        }

        #region useful_functions
        #region getType
        private TType getType(char input)
        {
            if (numbers.Contains(input))
                return TType.number;
            else if (operators.Contains(input))
                return TType.operation;
            else
                return TType.function;
        }
        #endregion

        #region countMaskinString
        private int countString(string target, string mask)
        {
            int result = 0;
            int pos;
            do
            {
                pos = target.IndexOf(mask);
                if (pos > -1)
                {
                    target = target.Substring(pos + mask.Length);
                    result++;
                }
            }
            while (pos > -1);


            return result;
        }
        #endregion

        #region getTokenValue
        private int getValue(String token)
        {
            switch (token)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                case "%":
                    return 2;
                case "^":
                    return 3;
                case "!":
                    return 5;
            }

            return 4;
        }
        #endregion

        #region preprocessing
        public string preprocessing(string infix)
        {
            //Fülle TokenList
            List<Token> tokenlist = new List<Token>();
            int i = 0;
            string temp = "";
            TType type = getType(infix[0]);
            TType lasttype = TType.operation;
            while (i < infix.Length)
            {
                type = getType(infix[i]);
                temp = "";

                //Get Token name
                while (type == getType(infix[i]))
                {

                    if (type == TType.function)
                    {
                        if (availableFunc.Contains(temp))
                        {
                            tokenlist.Add(new Token(temp, type));
                            temp = "";
                        }


                    }
                    temp += infix[i];

                    i++;
                    if (i == infix.Length)
                        break;
                }

                if (lasttype == TType.operation && type == TType.operation && temp == "-")
                {
                    tokenlist.Add(new Token("-1", TType.number));
                    tokenlist.Add(new Token("*", TType.operation));
                }
                else
                {
                    tokenlist.Add(new Token(temp, type));
                    lasttype = type;
                }
            }


            // Prüfe auf negative Zahlen und wandle um (-3 -> (0-3) )


            // Vollständigkeit der Klammern prüfen (Anzahl '(' == ')' )
            if (countString(infix, "(") != countString(infix, ")"))
                return ""; //Klammer Fehler !!!

            // Zahlen vor Klammern mit Operator verbinden ( 3( -> 3*( )           


            //Vollständigkeit der Operatoren

            infix = "";
            foreach (Token t in tokenlist)
            {

                infix += t.name;
            }

            return infix;
        }
#endregion

        #region getLastBracket
        private int getLastBracket(string sub)
        {
            int count = 0;
            for (int i = 0; i < sub.Length; i++)
            {
                if (sub[i] == ')')
                {
                    if (count == 0)
                        return i;
                    else
                        count--;
                }
                if (sub[i] == '(')
                    count++;
            }
            //Error Syntax
            return 0;
        }
        #endregion
        #endregion

        public double calculate(string infix)
        {
            //Calculate infix --> postfix
            /* Lese alle Zeichen der Eingabe ein.
             * Ist der Wert eine Zahl, so schreibe diese in das postfix
             * Ist es eine Operation, so schreibe diese in das op_stack, sofern der Wert der Operation 
             * größer als der Wert des letzten Wertes in der Tabelle ist.
             * Ist dem nicht so, schreibe alle Operationen aus dem Stack auf das postfix, welche größer sind als die gelesene Operation
             */
            List<Token> postfix = new List<Token>();
            List<String> stack = new List<String>();
            int i = 0;
            string temp = "";
            TType type = getType(infix[0]);
            TType lasttype = TType.operation;


            infix = infix.Trim();
            while (i < infix.Length)
            {
                type = getType(infix[i]);
                temp = "";

                //Get Token name
                while (type == getType(infix[i]))
                {
                    temp += infix[i];
                    i++;

                    if (i == infix.Length || type == TType.operation || infix[i - 1] == '(')
                        break;
                }


                //Negatives Vorzeichen zu -1* umgeschrieben (Bsp.: -3 => (-1)*3
                if (lasttype == TType.operation && type == TType.operation)
                {
                    if (temp == "-")
                    {
                        postfix.Add(new Token("-1", TType.number));
                        temp = "*";
                    }
                    else
                    {
                        postfix.Add(new Token("1", TType.number));
                        temp = "*";
                    }
                }

                //Fehlender Operator vor Funktionen wird ergänzt
                if ((type == TType.number && lasttype == TType.function) || (type == TType.function && lasttype == TType.number))
                {
                    i -= temp.Length;
                    type = TType.operation;
                    temp = "*";
                }


                //Add Token to Tokenlist
                switch (type)
                {
                    case TType.number:
                        postfix.Add(new Token(temp, type));
                        break;
                    case TType.operation:

                        for (int j = stack.Count - 1; j > -1; j--)
                        {
                            if (getValue(temp) > getValue(stack[j]))
                            {
                                stack.Add(temp);
                                break;
                            }
                            else
                            {
                                postfix.Add(new Token(stack[j], TType.operation));
                                stack.RemoveAt(j);
                            }
                        }

                        if (stack.Count == 0)
                            stack.Add(temp);
                        break;
                    case TType.function:
                        if (availableFunc.Contains(temp))
                        {
                            Function func = new Function(temp);
                            string sub = infix.Substring(i); 
                            //Funktion get last bracket
                            int pos = getLastBracket(sub);
                            //int pos = sub.LastIndexOf(")");
                            sub = sub.Substring(0, pos);
                            while (sub.IndexOf(",") != -1)
                            {
                                int pos2 = sub.IndexOf(",");
                                if (func.term.Length == 0 && temp == "integral(")
                                    func.term = sub.Substring(0, pos2);
                                else
                                    func.parameters.Add(calculate(sub.Substring(0, pos2)));
                                sub = sub.Substring(pos2 + 1);
                            }
                            func.parameters.Add(calculate(sub));
                            i += pos + 1;

                            postfix.Add(new Token(func.calculate().ToString(CultureInfo.CreateSpecificCulture("en-GB")), TType.number));
                            type = TType.number;
                        }
                        if (variables != null)
                            foreach (Variable v in variables)
                            {
                                if (temp == v.name)
                                {
                                    postfix.Add(new Token(v.wert.ToString(CultureInfo.CreateSpecificCulture("en-GB")), TType.number));
                                    temp = "";
                                    type = TType.number;
                                }

                            }
                        break;
                }
                lasttype = type;
            }


            //Add operation stack to postfix
            for (int j = stack.Count - 1; j > -1; j--)
            {
                postfix.Add(new Token(stack[j], TType.operation));
                stack.RemoveAt(j);
            }
            //Calculate postfix--> result
            /* Lese alle Tokens der postfix Liste nacheinander ein.
             * Schreibe alle Zahlen in einen Stack, wird eine Operation gelesen, so führe die Operation mit den letzten 
             * beiden hinzugefügten Zahlen aus, lösche die beiden Zahlen und ersetze sie mit ihrem Ergebnis
             */
            double result = 0;
            for (i = 0; i < postfix.Count; i++)
            {
                switch (postfix[i].type)
                {
                    case TType.number:
                        stack.Add(postfix[i].name);
                        break;
                    case TType.operation:
                        switch (postfix[i].name)
                        {
                            case "+":
                                result = Convert.ToDouble(stack[stack.Count - 2], CultureInfo.CreateSpecificCulture("en-GB"))
                                         + Convert.ToDouble(stack[stack.Count - 1], CultureInfo.CreateSpecificCulture("en-GB"));
                                break;
                            case "-":
                                result = Convert.ToDouble(stack[stack.Count - 2], CultureInfo.CreateSpecificCulture("en-GB"))
                                         - Convert.ToDouble(stack[stack.Count - 1], CultureInfo.CreateSpecificCulture("en-GB"));
                                break;
                            case "*":
                                result = Convert.ToDouble(stack[stack.Count - 2], CultureInfo.CreateSpecificCulture("en-GB"))
                                         * Convert.ToDouble(stack[stack.Count - 1], CultureInfo.CreateSpecificCulture("en-GB"));
                                break;
                            case "/":
                                result = Convert.ToDouble(stack[stack.Count - 2], CultureInfo.CreateSpecificCulture("en-GB"))
                                         / Convert.ToDouble(stack[stack.Count - 1], CultureInfo.CreateSpecificCulture("en-GB"));
                                break;
                            case "%":
                                result = Convert.ToDouble(stack[stack.Count - 2], CultureInfo.CreateSpecificCulture("en-GB"))
                                         % Convert.ToDouble(stack[stack.Count - 1], CultureInfo.CreateSpecificCulture("en-GB"));
                                break;
                            case "^":
                                result = Math.Pow(Convert.ToDouble(stack[stack.Count - 2], CultureInfo.CreateSpecificCulture("en-GB")),
                                         Convert.ToDouble(stack[stack.Count - 1], CultureInfo.CreateSpecificCulture("en-GB")));
                                break;
                        }

                        stack.RemoveAt(stack.Count - 2);
                        stack.RemoveAt(stack.Count - 1);
                        stack.Add(result.ToString(CultureInfo.CreateSpecificCulture("en-GB")));
                        break;

                }
            }

            return Convert.ToDouble(stack[0], CultureInfo.CreateSpecificCulture("en-GB"));
        }
    }

}
