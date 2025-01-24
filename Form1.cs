using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Calculatrice
{
    public partial class btnBackspace : Form
    {
        // Variables internes pour l'ancienne logique (on peut les garder si utile)
        private double firstNumber = 0;
        private string currentOperator = "";
        private bool isOperatorClicked = false;

        public btnBackspace()
        {
            InitializeComponent();
        }

        // =============================
        //  GESTION DE L'INTERFACE
        // =============================

        // Boutons "chiffres"
        private void NumberButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            txtTotal.Text += button.Text;
        }

        // Boutons "opérateurs" simples (+, -, x, /, ^)
        private void OperatorButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            txtTotal.Text += " " + button.Text + " ";
            // On insère des espaces autour pour mieux séparer les tokens, ex: "... + ..."
        }

        // Bouton "point décimal"
        private void btndot_Click(object sender, EventArgs e)
        {
            txtTotal.Text += ".";
        }

        // Parenthèses
        private void btnpar1_Click(object sender, EventArgs e)
        {
            txtTotal.Text += "(";
        }

        private void btnpar2_Click(object sender, EventArgs e)
        {
            txtTotal.Text += ")";
        }

        // Fonctions "sin", "cos", "tan", "log", "sqrt" :
        // On insère directement "sin(" ou "cos(" ... 
        // L'utilisateur finira la parenthèse et cliquera sur "="

        // Sinus en degrés
        private void btnOct_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtTotal.Text, out double degrees))
            {
                // Conversion degré -> radians
                double radians = degrees * Math.PI / 180.0;
                double resultat = Math.Sin(radians);

                // Afficher le résultat
                txtTotal.Text = resultat.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez entrer un angle valide (en degrés) pour le sinus.");
            }
        }



        // CALCUL DU COS
        private void btnDec_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtTotal.Text, out double degrees))
            {
                // Conversion degré -> radians  
                double radians = degrees * Math.PI / 180.0;
                double resultat = Math.Cos(radians);

                // Afficher le résultat
                txtTotal.Text = resultat.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez entrer un angle valide (en degrés) pour le cosinus.");
            }
        }


        private void btnsin_Click(object sender, EventArgs e)
        {
            txtTotal.Text += "sin(";
        }

        private void btncos_Click(object sender, EventArgs e)
        {
            txtTotal.Text += "cos(";
        }

        private void btntan_Click(object sender, EventArgs e)
        {
            txtTotal.Text += "tan(";
        }

        private void btnlog_Click(object sender, EventArgs e)
        {
            txtTotal.Text += "log(";
        }

        // Ici on appelle "btnBin", mais on va lui faire insérer "sqrt(" 
        private void btnBin_Click(object sender, EventArgs e)
        {
            txtTotal.Text += "sqrt(";
        }

        // Bouton CLEAR
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtTotal.Text = "";
        }

        // =============================
        //  BOUTON "=" : EVALUATION
        // =============================
        private void btnEql_Click(object sender, EventArgs e)
        {
            try
            {
                string expression = txtTotal.Text;
                // Petits nettoyages, ex: remplacer 'x' par '*'
                expression = expression.Replace("x", "*");

                double result = EvaluateExpression(expression);
                txtTotal.Text = result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur de calcul : " + ex.Message);
            }
        }

        // =============================
        //  METHODE DE PARSING & EVAL
        // =============================
        /// <summary>
        /// Evalue une expression mathématique contenant 
        /// +, -, *, /, ^, sin, cos, tan, log, sqrt, et parenthèses.
        /// Les fonctions trigonométriques sont traitées en DEGRES.
        /// </summary>
        private double EvaluateExpression(string expression)
        {
            // 1) Convertir l'expression en liste de tokens
            List<string> tokens = Tokenize(expression);

            // 2) Convertir en notation postfixée (RPN) via l'algorithme Shunting-yard
            List<string> rpn = ShuntingYard(tokens);

            // 3) Evaluer la liste RPN
            return EvaluateRPN(rpn);
        }

        // ------------------------------
        //  (1) TOKENIZE
        // ------------------------------
        private List<string> Tokenize(string expr)
        {
            // On va lire caractère par caractère et regrouper en tokens
            // ex: "sin(45) + 2.5" -> ["sin","(","45",")","+","2.5"]
            // On sépare aussi les opérateurs et parenthèses
            // NB : on admet que l'utilisateur peut mettre des espaces, on va les ignorer.

            List<string> tokens = new List<string>();
            int i = 0;
            while (i < expr.Length)
            {
                char c = expr[i];

                // Ignorer les espaces
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // Parenthèses
                if (c == '(' || c == ')')
                {
                    tokens.Add(c.ToString());
                    i++;
                }
                // Opérateurs simples
                else if ("+-*/^".IndexOf(c) >= 0)
                {
                    tokens.Add(c.ToString());
                    i++;
                }
                // Fonctions (sin, cos, tan, log, sqrt)
                else if (char.IsLetter(c))
                {
                    // Récupérer la suite de lettres (ex: "sin", "cos", "sqrt"…)
                    int start = i;
                    while (i < expr.Length && char.IsLetter(expr[i]))
                        i++;
                    string func = expr.Substring(start, i - start).ToLower();
                    tokens.Add(func);
                }
                // Nombre (entier ou décimal)
                else if (char.IsDigit(c) || c == '.')
                {
                    int start = i;
                    bool hasDot = (c == '.');
                    i++;
                    while (i < expr.Length)
                    {
                        if (char.IsDigit(expr[i]))
                        {
                            i++;
                        }
                        else if (expr[i] == '.' && !hasDot)
                        {
                            hasDot = true;
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    string number = expr.Substring(start, i - start);
                    tokens.Add(number);
                }
                else
                {
                    // Caractère non reconnu
                    throw new Exception("Caractère non reconnu: " + c);
                }
            }

            return tokens;
        }

        // ------------------------------
        //  (2) SHUNTING-YARD
        // ------------------------------
        private List<string> ShuntingYard(List<string> tokens)
        {
            // Convertit la liste de tokens en notation postfixée (RPN).
            // - on gère la priorité des opérateurs
            // - on place les fonctions avant l'argument
            // - on gère les parenthèses
            Stack<string> stack = new Stack<string>();
            List<string> output = new List<string>();

            foreach (string token in tokens)
            {
                if (IsNumber(token))
                {
                    // Les nombres vont directement dans la sortie
                    output.Add(token);
                }
                else if (IsFunction(token))
                {
                    // Les fonctions vont sur la pile
                    stack.Push(token);
                }
                else if (IsOperator(token))
                {
                    // Tant qu'il y a un opérateur/fonction au sommet de la pile
                    // avec une priorité >= au token courant, on le pop
                    while (stack.Count > 0 &&
                           (IsOperator(stack.Peek()) || IsFunction(stack.Peek())) &&
                           (GetPrecedence(stack.Peek()) >= GetPrecedence(token)))
                    {
                        output.Add(stack.Pop());
                    }
                    // puis on push l'opérateur courant
                    stack.Push(token);
                }
                else if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    // Dépiler jusqu'à "("
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }
                    if (stack.Count == 0)
                        throw new Exception("Parenthèses mismatched : ) sans (");

                    // Pop la "("
                    stack.Pop();

                    // Si juste après on avait une fonction au sommet, on la pop aussi
                    if (stack.Count > 0 && IsFunction(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                }
                else
                {
                    throw new Exception("Token non géré: " + token);
                }
            }

            // Vider la pile
            while (stack.Count > 0)
            {
                string top = stack.Pop();
                if (top == "(" || top == ")")
                    throw new Exception("Parenthèses mismatched : il manque une )");
                output.Add(top);
            }

            return output;
        }
        private void txtTotal_TextChanged(object sender, EventArgs e)
        {
            // Laissez vide ou placez ici le code à exécuter lors du changement de texte
        }

        // ------------------------------
        //  (3) EVALUER la RPN
        // ------------------------------
        private double EvaluateRPN(List<string> rpn)
        {
            Stack<double> stack = new Stack<double>();

            foreach (var token in rpn)
            {
                if (IsNumber(token))
                {
                    stack.Push(double.Parse(token, CultureInfo.InvariantCulture));
                }
                else if (IsOperator(token))
                {
                    // Il nous faut 2 opérandes
                    if (stack.Count < 2)
                        throw new Exception("Pas assez d'opérandes pour l'opérateur " + token);

                    double b = stack.Pop();
                    double a = stack.Pop();

                    double result = 0;
                    switch (token)
                    {
                        case "+": result = a + b; break;
                        case "-": result = a - b; break;
                        case "*": result = a * b; break;
                        case "/":
                            if (b == 0)
                                throw new DivideByZeroException("Division par zéro !");
                            result = a / b;
                            break;
                        case "^":
                            result = Math.Pow(a, b);
                            break;
                    }
                    stack.Push(result);
                }
                else if (IsFunction(token))
                {
                    // Il nous faut 1 opérande
                    if (stack.Count < 1)
                        throw new Exception("Pas assez d'opérandes pour la fonction " + token);

                    double x = stack.Pop();
                    double res = 0;

                    // Pour sin, cos, tan : on convertit x (en degrés) -> radians
                    double rad = x * Math.PI / 180.0;

                    switch (token)
                    {
                        case "sin":
                            res = Math.Sin(rad);
                            break;
                        case "cos":
                            res = Math.Cos(rad);
                            break;
                        case "tan":
                            // Attention à tan(90) par ex => ∞
                            res = Math.Tan(rad);
                            break;
                        case "log":
                            if (x <= 0)
                                throw new Exception("log10 non défini pour " + x);
                            res = Math.Log10(x);
                            break;
                        case "sqrt":
                            if (x < 0)
                                throw new Exception("sqrt non défini pour " + x);
                            res = Math.Sqrt(x);
                            break;
                        default:
                            throw new Exception("Fonction inconnue: " + token);
                    }

                    stack.Push(res);
                }
                else
                {
                    throw new Exception("Token RPN inconnu: " + token);
                }
            }

            if (stack.Count != 1)
                throw new Exception("Expression invalide (pile finale != 1).");

            return stack.Pop();
        }

        // =============================
        //  FONCTIONS UTILITAIRES
        // =============================
        private bool IsNumber(string token)
        {
            // Test rapide si c'est un nombre (double)
            // Evite log( etc. 
            // On n'utilise pas TryParse directement sur "sin" etc.
            return double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        private bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" || token == "^";
        }

        private bool IsFunction(string token)
        {
            // On gère en minuscules
            switch (token)
            {
                case "sin":
                case "cos":
                case "tan":
                case "log":
                case "sqrt":
                    return true;
            }
            return false;
        }

        private int GetPrecedence(string token)
        {
            // Priorités : 
            //   1 pour + -
            //   2 pour * /
            //   3 pour ^ (exponent)
            //   4 pour les fonctions (on les pop direct sur la pile)
            if (IsFunction(token)) return 4;

            switch (token)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                    return 2;
                case "^":
                    return 3;
            }
            return 0;
        }

        // =============================
        //  EVENEMENTS PAR DEFAUT
        // =============================
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // Si tu utilises des boutons 0..9, assure-toi qu'ils appellent NumberButton_Click
        private void btn1_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void btn2_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void btn3_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void btn4_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void btn5_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void btn6_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void btn7_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void btn8_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void btn9_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void btn0_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);

        private void btnPlus_Click(object sender, EventArgs e) => OperatorButton_Click(sender, e);
        private void btnMin_Click(object sender, EventArgs e) => OperatorButton_Click(sender, e);
        private void btnMul_Click(object sender, EventArgs e) => OperatorButton_Click(sender, e);
        private void btnDiv_Click(object sender, EventArgs e) => OperatorButton_Click(sender, e);
        private void btnpuissance_Click(object sender, EventArgs e) => OperatorButton_Click(sender, e);

        private void button2_Click(object sender, EventArgs e)
        {
            
        }
    }
}
