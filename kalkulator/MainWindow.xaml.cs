/**
 * Prosty kalkulator w technologii WPF na Programowanie Obiektowe i Graficzne
 * Daniel Wojdak
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kalkulator
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        char[] operators; //possible operators
        string divError; //error when dividing by 0
        char separator; //decimal separator
        public MainWindow()
        {
            InitializeComponent();
            operators = new char[]{ '+', '-', 'x', '/', '^' };
            divError = "Nie można dzielić przez zero!";
            separator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            sepBtn.Content = separator; //set separator in gui
        }

        // add number or operator to equation
        private void commonButtonClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string equationContent = equation.Content.ToString();
            string btnContent = btn.Content.ToString();
            bool space=false;

            //check if error is displayed
            if (equationContent == divError) equationContent = "";

            if (equationContent.Length > 0)
            {
                if (operators.Contains(equationContent.Last())) // last element of equation is operator
                {
                    if (operators.Contains(btnContent[0])) // change operator
                    {
                        equationContent = equationContent.Substring(0, equationContent.Length - 1);
                    }
                    else
                    {
                        space = true;
                        if (btnContent[0] == separator) btnContent = "0" + separator; //add 0 when entering , alone
                    }
                }
                else if (operators.Contains(btnContent[0])) // adding operator to equation
                {
                    if (hasTwoNumbers(equationContent))
                    {
                        calculate();
                        equationContent = equation.Content.ToString(); //update equation content
                    }
                    space = true;
                    if (equationContent.Last() == separator) //remove , from last int number
                        equationContent = equationContent.Substring(0, equationContent.Length - 1);
                    if(equationContent.Last()==' ') //remove whitespace after using delete() and change operator
                    {
                        space = false;
                        equationContent = equationContent.Substring(0, equationContent.Length - 2);
                    }

                }
                else
                {
                    space = false; // adding next digit of number
                    if (btnContent[0] == separator && isDouble(equationContent)) return; //check if number is already double and block adding next ,
                }
            }
            else //equation is empty
            {
                space = false;
                if (btnContent[0] == separator) btnContent = "0" + separator; //add 0 when entering , alone
                else if (operators.Contains(btnContent.Last())) return; // operator can't be first
            }

            if(space) equation.Content = $"{equationContent} {btnContent}";
            else equation.Content = $"{equationContent}{btnContent}";
        }

        private void clear(object sender, RoutedEventArgs e)
        {
            equation.Content = "";
        }

        private void delete(object sender, RoutedEventArgs e)
        {
            string equationContent = equation.Content.ToString();
            //first check if error is displayed
            if (equationContent == divError) equationContent = "";
            else if (equationContent.Length > 0)
            {
                if (equationContent.Length > 1)
                {
                    if (operators.Contains(equationContent.Last())) // last element of equation is operator
                        equationContent = equationContent.Substring(0, equationContent.Length - 2);
                    else if (equationContent.Last() == ' ') // last element of equation is whitespace after using delete()
                        equationContent = equationContent.Substring(0, equationContent.Length - 3);
                    else if (equationContent[equationContent.Length - 2] == '-') //delete digit with negation
                        equationContent = equationContent.Substring(0, equationContent.Length - 2);
                    else equationContent = equationContent.Substring(0, equationContent.Length - 1); // delete just one digit
                }
                // delete first and only digit of equation
                else
                    equationContent = "";
            }
            //do nothing if equation is empty
            equation.Content = equationContent;
        }

        // opposite value
        private void negate(object sender, RoutedEventArgs e)
        {
            string equationContent = equation.Content.ToString();
            if (equationContent.Length == 0) return; // do nothing if equation is empty

            int i;
            for (i = equationContent.Length - 1; i >= 0; i--)
            {
                if (equationContent[i] != ' ' && !operators.Contains(equationContent[i])) continue; //finding potential number
                else if (operators.Contains(equationContent[i]) && i - 1 >= 0 && equationContent[i - 1] == ' ') //manage operastor
                {
                    //check if - is part of number, if so, change to no negative
                    if (i + 1 < equationContent.Length)
                    {
                        equationContent = equationContent.Remove(i, 1);
                    }
                    //equation operator:
                    //change - to +
                    else if (equationContent[i] == '-')
                    {
                        equationContent = equationContent.Remove(i, 1);
                        equationContent = equationContent.Insert(i, "+");
                    }
                    //change + to -
                    else if (equationContent[i] == '+')
                    {
                        equationContent = equationContent.Remove(i, 1);
                        equationContent = equationContent.Insert(i, "-");
                    }
                    //otherwise do nothing. number can be negated only after adding to equation
                    break;
                }
                else if (equationContent.Last() == ' ') break; // when whitespace is last element => there is operator before it
                else
                {
                    if (equationContent[i] == '-') equationContent = equationContent.Remove(i, 1);
                    else equationContent = equationContent.Insert(i + 1, "-");
                    break;
                }
            }
            //if for ended and nothing has been changed add - at the begining
            if (i == -1) equationContent = equationContent.Insert(0, "-");
            equation.Content = equationContent;
        }

        private void calculateBtn(object sender, RoutedEventArgs e)
        {
            calculate();
        }

        private void calculate()
        {
            string equationContent = equation.Content.ToString();
            string[] tab = equationContent.Split(' ');

            if(tab.Count() == 3) // equation has to have 3 parts: number_operator_number
            {
                //get numbers
                double n1 = Convert.ToDouble(tab[0]);
                double n2 = Convert.ToDouble(tab[2]);
                //get operator
                char op = tab[1][0];

                double result;
                switch (op)
                {
                    case '+': result = n1 + n2; break;
                    case '-': result = n1 - n2; break;
                    case 'x': result = n1 * n2; break;
                    case '/':
                        if (n2 != 0) result = n1 / n2;
                        else
                        {
                            equation.Content = divError;
                            return;
                        }
                        break;
                    case '^': result = Math.Pow(n1, n2); break;
                    default: result = 0; break;
                }
                //replace separator
                equation.Content = result.ToString();
            }
        }

        private bool hasTwoNumbers(string equation)
        {
            string[] tab = equation.Split(' ');
            if (tab.Length == 3)
            {
                if (tab[2].Length > 0) return true;
                else return false;
            }
            else return false;
        }

        private bool isDouble(string equation)
        {
            for (int i = equation.Length - 1; i >= 0; i--)
            {
                if (equation[i] == ' ' | equation[i] == '-') break; //finded number
                else
                {
                    if (equation[i] == separator) return true;
                }
            }
            return false;
        }
    }
}
