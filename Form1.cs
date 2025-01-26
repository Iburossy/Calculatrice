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
        // Historique (liste des opérations) :
        private List<string> history = new List<string>();

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

        // =============================
        //  EXEMPLES DE FONCTIONS TRIGO
        // =============================

        // Sinus en degrés (ancien code)
        private void btnOct_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtTotal.Text, out double degrees))
            {
                double radians = degrees * Math.PI / 180.0;
                double resultat = Math.Sin(radians);
                txtTotal.Text = resultat.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez entrer un angle valide (en degrés) pour le sinus.");
            }
        }

        // Cosinus (ancien code)
        private void btnDec_Click(object sender, EventArgs e)
        {
            if (double.TryParse(txtTotal.Text, out double degrees))
            {
                double radians = degrees * Math.PI / 180.0;
                double resultat = Math.Cos(radians);
                txtTotal.Text = resultat.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez entrer un angle valide (en degrés) pour le cosinus.");
            }
        }

        private void btnsin_Click(object sender, EventArgs e) => txtTotal.Text += "sin(";
        private void btncos_Click(object sender, EventArgs e) => txtTotal.Text += "cos(";
        private void btntan_Click(object sender, EventArgs e) => txtTotal.Text += "tan(";
        private void btnlog_Click(object sender, EventArgs e) => txtTotal.Text += "log(";

        // "sqrt(" via btnBin
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

                // AJOUT POUR L'HISTORIQUE :
                string historiqueEntree = expression + " = " + result;
                history.Add(historiqueEntree);

                // Limiter à 5 entrées max :
                if (history.Count > 5)
                {
                    history.RemoveAt(0); // supprime la plus ancienne
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur de calcul : " + ex.Message);
            }
        }

        // =============================
        //  METHODE DE PARSING & EVAL
        // =============================
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
            List<string> tokens = new List<string>();
            int i = 0;
            while (i < expr.Length)
            {
                char c = expr[i];

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
                // AJOUT : on inclut maintenant aussi '%'
                else if ("+-*/^%".IndexOf(c) >= 0)
                {
                    tokens.Add(c.ToString());
                    i++;
                }
                // Fonctions (sin, cos, tan, log, sqrt)
                else if (char.IsLetter(c))
                {
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
            Stack<string> stack = new Stack<string>();
            List<string> output = new List<string>();

            foreach (string token in tokens)
            {
                if (IsNumber(token))
                {
                    output.Add(token);
                }
                else if (IsFunction(token))
                {
                    stack.Push(token);
                }
                else if (IsOperator(token))
                {
                    while (stack.Count > 0 &&
                           (IsOperator(stack.Peek()) || IsFunction(stack.Peek())) &&
                           (GetPrecedence(stack.Peek()) >= GetPrecedence(token)))
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
                else if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }
                    if (stack.Count == 0)
                        throw new Exception("Parenthèses mismatched : ) sans (");

                    // Pop la "("
                    stack.Pop();

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
            // Laissez vide ou placez ici du code si vous souhaitez réagir au changement de texte
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
                        // AJOUT : gestion du modulo
                        case "%":
                            if (b == 0)
                                throw new DivideByZeroException("Modulo par zéro !");
                            result = a % b;
                            break;
                    }
                    stack.Push(result);
                }
                else if (IsFunction(token))
                {
                    if (stack.Count < 1)
                        throw new Exception("Pas assez d'opérandes pour la fonction " + token);

                    double x = stack.Pop();
                    double res = 0;
                    double rad = x * Math.PI / 180.0;

                    switch (token)
                    {
                        case "sin": res = Math.Sin(rad); break;
                        case "cos": res = Math.Cos(rad); break;
                        case "tan": res = Math.Tan(rad); break;
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
            return double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        private bool IsOperator(string token)
        {
            // AJOUT : on inclut maintenant '%'
            return token == "+" || token == "-" || token == "*" || token == "/" || token == "^" || token == "%";
        }

        private bool IsFunction(string token)
        {
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
            // On donne une priorité au modulo identique à * et / (2)
            if (IsFunction(token)) return 4;

            switch (token)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                case "%":   // AJOUT
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
            if (!string.IsNullOrEmpty(txtTotal.Text))
            {
                // Supprime le dernier caractère
                txtTotal.Text = txtTotal.Text.Remove(txtTotal.Text.Length - 1, 1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Cos supplémentaire
            if (double.TryParse(txtTotal.Text, out double degrees))
            {
                double radians = degrees * Math.PI / 180.0;
                double resultat = Math.Cos(radians);
                txtTotal.Text = resultat.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez entrer un angle valide (en degrés) pour le cosinus.");
            }
        }

        // =============================
        //   AJOUT DES 6 NOUVELLES FONCTIONS
        // =============================

        // 1) Modulo
        private void btnModu_Click(object sender, EventArgs e)
        {
            // Même approche que +, -, etc. On insère " % " dans l'expression.
            txtTotal.Text += " % ";
        }

        // 2) Pourcentage
        private void btnPer_Click(object sender, EventArgs e)
        {
            // Convertit la valeur dans txtTotal en pourcentage => val / 100.
            if (double.TryParse(txtTotal.Text, out double val))
            {
                double result = val / 100.0;
                txtTotal.Text = result.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez entrer un nombre valide pour calculer le pourcentage.");
            }
        }

        // 3) Conversion en Décimal
        private void btnDeci_Click(object sender, EventArgs e)
        {
            // On suppose que txtTotal contient un entier base 10, ou on retente un parse
            if (int.TryParse(txtTotal.Text, out int decimalValue))
            {
                // réaffiche en base 10
                txtTotal.Text = decimalValue.ToString();
            }
            else
            {
                MessageBox.Show("La valeur affichée n'est pas un entier décimal valide.");
            }
        }

        // 4) Conversion en Binaire
        private void btnBinaire_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtTotal.Text, out int decimalValue))
            {
                txtTotal.Text = Convert.ToString(decimalValue, 2);
            }
            else
            {
                MessageBox.Show("La valeur affichée n'est pas un entier décimal valide.");
            }
        }

        // 5) Conversion en Octal
        private void btnOctal_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtTotal.Text, out int decimalValue))
            {
                txtTotal.Text = Convert.ToString(decimalValue, 8);
            }
            else
            {
                MessageBox.Show("La valeur affichée n'est pas un entier décimal valide.");
            }
        }

        // 6) Conversion en Hexadécimal
        private void btnHexad_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtTotal.Text, out int decimalValue))
            {
                txtTotal.Text = Convert.ToString(decimalValue, 16).ToUpper();
            }
            else
            {
                MessageBox.Show("La valeur affichée n'est pas un entier décimal valide.");
            }
        }

        private void btnOff_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnHis_Click(object sender, EventArgs e)
        {
            if (history.Count == 0)
            {
                MessageBox.Show("Aucun calcul effectué pour le moment.", "Historique");
            }
            else
            {
                // Concaténer chaque ligne de l'historique sur une nouvelle ligne
                string message = string.Join(Environment.NewLine, history);
                MessageBox.Show(message, "Historique des 5 derniers calculs");
            }
        }
    }
}
