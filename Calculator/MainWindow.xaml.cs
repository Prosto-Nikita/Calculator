using System;
using System.Data;
using System.Collections.Generic;
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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Data.SqlTypes;
using System.Windows.Controls.Primitives;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Добавление общего события для обработки нажатия всех кнопок калькулятора 
            foreach(UIElement el in MainPanel.Children)
            {
                if(el is Button)
                {
                    ((Button)el).Click += Button_Click;
                }
            }
        }
        //Объявление списка для всех вводимых оперторов и операндов
        List<string> calculation_line = new List<string>();
        //Переменная с значением 0 для последующего использования в TryParse
        int Null = 0;
        double NullD = 0;
        //Обработка активации CheckBox (включение инженерного режима)
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            this.Height += 80;
            foreach (UIElement el in MainPanel.Children)
            {
                if (el is Button && el.Visibility==Visibility.Visible && ((Button)el).Name!="ButtonExit")
                {
                    Thickness currentMargin = ((Button)el).Margin;
                    ((Button)el).Margin=new Thickness(currentMargin.Left, currentMargin.Top + 80, currentMargin.Right, currentMargin.Bottom);
                }
            }
            ButtonSin.Visibility = Visibility.Visible;
            ButtonCos.Visibility = Visibility.Visible;
            ButtonTg.Visibility = Visibility.Visible;
            ButtonLn.Visibility = Visibility.Visible;
            ButtonSqrt.Visibility = Visibility.Visible;
        }
        //Обработка дизактивации CheckBox (выключение инженерного режима)
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Height -= 80;
            foreach (UIElement el in MainPanel.Children)
            {
                if (el is Button && el.Visibility == Visibility.Visible && ((Button)el).Name != "ButtonExit" && ((Button)el).Name != "ButtonExit" && ((Button)el).Name != "ButtonCos" && ((Button)el).Name != "ButtonSin" && ((Button)el).Name != "ButtonTg" && ((Button)el).Name != "ButtonLn" && ((Button)el).Name != "ButtonSqrt")
                {
                    Thickness currentMargin = ((Button)el).Margin;
                    ((Button)el).Margin = new Thickness(currentMargin.Left, currentMargin.Top - 80, currentMargin.Right, currentMargin.Bottom);
                }
            }
            ButtonSin.Visibility = Visibility.Hidden;
            ButtonCos.Visibility = Visibility.Hidden;
            ButtonTg.Visibility = Visibility.Hidden;
            ButtonLn.Visibility = Visibility.Hidden;
            ButtonSqrt.Visibility = Visibility.Hidden;
        }
        //Обработка нажатия любой кнопки на калькуляторе
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Скрытие дисплея ошибок и возращение основного дисплея калькулятора
            DisplayError.Visibility = Visibility.Hidden;
            Display.Visibility = Visibility.Visible;

            //Запись в переменную значения, которое хранит нажатая кнопка
            string str = (string)((Button)e.OriginalSource).Content;

            if ((string)Display.Content == "0") { Display.Content = ""; calculation_line.Clear(); }
            if (str == "AC") {//Очистить всё
                Display.Content = "0";
                calculation_line.Clear();
                calculation_line.Add("0");
            }
            else if (str == "=") {//Равно
                //Копия списка на случай ошибки
                List<string> CopyList = calculation_line.ToList();
                try
                {
                    //Проверка на наличие чисел
                    bool IsNumber = false;
                    for (int i = 0; i < calculation_line.Count; i++)
                    {
                        if (double.TryParse(calculation_line[i], out NullD) || calculation_line[i].Contains("E"))
                        {
                            IsNumber = true;
                        }
                    }
                    if (!IsNumber) { throw new Exception("Ошибка! А где собcтвенно числа?"); }
                        //Проверка правильности чисел к научной нотации
                    for (int i = 0; i < calculation_line.Count; i++)
                    {
                        if (calculation_line[i].Contains('E'))
                        {
                            calculation_line[i] = Convert.ToDouble(calculation_line[i]).ToString("E2");
                        }
                    }
                    for(int i = 0; i < calculation_line.Count; i++)
                    {
                        for(int j=0;j<calculation_line[i].Length; j++)
                        {
                            if (Convert.ToString(calculation_line[i][j]) == "E" && calculation_line[i].Length < 9) {
                                calculation_line[i].Insert(6, "0");
                                i--;
                            }
                        }
                    }
                    //Проверка выражение на правильность расставления скобок
                    int ClosingBrackets = 0;
                    for (int i = 0; i < calculation_line.Count; i++) {
                        if (calculation_line[i] == "(") { ClosingBrackets++; }
                        if (calculation_line[i]==")") { ClosingBrackets--;
                            if (ClosingBrackets < 0)
                            {
                                throw new Exception("Ошибка! Неправильно расставлены\n скобки.");
                            }
                        }
                    }
                    //Добавление закрывающих скобок при их недостатке
                    if(ClosingBrackets != 0) {
                        while (ClosingBrackets != 0) {
                            calculation_line.Add(")");
                            ClosingBrackets--;
                            Display.Content += ")";
                        }
                        return;
                    }
                    if (calculation_line.Count == 0) { return; }
                    double t = 0;
                    if ((string)Display.Content != "0" && (string)Display.Content != "")
                    {
                        int begin, end;
                        while (calculation_line.Count != 1 && calculation_line.Count != 0)
                        {
                            begin = 0;
                            end = calculation_line.Count - 1;
                            //Выделение выражения заключенного в скобки
                            for(int a = 0; a < calculation_line.Count; a++)
                            {
                                if (calculation_line[a] == "(")
                                {
                                    begin = a;
                                }
                                if (calculation_line[a] == ")")
                                {
                                    end = a;
                                    end--;
                                    calculation_line.RemoveAt(begin);
                                    calculation_line.RemoveAt(end);
                                    end--;
                                    break;
                                }
                            }
                            double temp;
                            //Решение выделенного выражения
                            for (int i = begin; i <= end; i++)
                            {
                                /*Операнды первого приоритета*/
                                if (calculation_line[i] == "sqrt") {//Вычисление корня
                                    if (Convert.ToDouble(calculation_line[i + 1]) < 0)
                                    {
                                        throw new Exception("Ошибка! Отрицательное число в корне.");
                                    }
                                    calculation_line[i] = Convert.ToString(Math.Sqrt(Convert.ToDouble(calculation_line[i + 1])));
                                    calculation_line.RemoveAt(i + 1);
                                    i = begin;
                                    break;
                                }
                                else if (calculation_line[i] == "sin") {//Вычисление синуса
                                    calculation_line[i] = Convert.ToString(Math.Sin(Convert.ToDouble(calculation_line[i + 1])));
                                    calculation_line.RemoveAt(i + 1);
                                    i = begin;
                                    break;
                                }
                                else if (calculation_line[i] == "cos") {//Вычисление косинуса
                                    calculation_line[i] = Convert.ToString(Math.Cos(Convert.ToDouble(calculation_line[i + 1])));
                                    calculation_line.RemoveAt(i + 1);
                                    i = begin;
                                    break;
                                }
                                else if (calculation_line[i] == "tg") {//Вычисление тангенса
                                    calculation_line[i] = Convert.ToString(Math.Tan(Convert.ToDouble(calculation_line[i + 1])));
                                    calculation_line.RemoveAt(i + 1);
                                    i = begin;
                                    break;
                                }
                                else if (calculation_line[i] == "ln") {//Вычисление натурального логарифма
                                    if (Convert.ToDouble(calculation_line[i + 1]) <= 0)
                                    {
                                        throw new Exception("Ошибка! Отрицательное число \nв логарифме.");
                                    }
                                    calculation_line[i] = Convert.ToString(Math.Log(Convert.ToDouble(calculation_line[i + 1])));
                                    calculation_line.RemoveAt(i + 1);
                                    i = begin;
                                    break;
                                }
                                /*Если ни корень, ни синус, ни косинус, ни тангенс и ни корень не встреитись,
                                 * то переход к следующим операндам*/
                                if (i == end)
                                {
                                    /*Операнды второго приоритета*/
                                    for (int j = begin; j <= end; j++)
                                    {
                                        if (calculation_line[j] == "*" || calculation_line[j] == "/")
                                        {
                                            if (j == begin) { throw new Exception("Ошибка! Оператор без операнда."); }
                                            if (j == end) { throw new Exception("Ошибка! Оператор без операнда."); }
                                            if (calculation_line[j] == "*")
                                            {
                                                temp = Convert.ToDouble(calculation_line[j - 1]) * Convert.ToDouble(calculation_line[j + 1]);
                                                calculation_line.RemoveAt(j - 1);
                                                calculation_line[j - 1] = temp.ToString();
                                                calculation_line.RemoveAt(j);
                                                j = begin;
                                                break;
                                            }
                                            else
                                            {
                                                if (Convert.ToDouble(calculation_line[j + 1]) == 0)
                                                {
                                                    throw new Exception("Ошибка! Деление на ноль.");
                                                }
                                                temp = Convert.ToDouble(calculation_line[j - 1]) / Convert.ToDouble(calculation_line[j + 1]);
                                                calculation_line.RemoveAt(j - 1);
                                                calculation_line[j - 1] = temp.ToString();
                                                calculation_line.RemoveAt(j);
                                                j = begin;
                                                break;
                                            }
                                        }
                                        if (j == end)
                                        {
                                            /*Операнды третьего приоритета*/
                                            for (int k = begin; k <= end; k++)
                                            {
                                                if (calculation_line[k] == "+" || calculation_line[k] == "-")
                                                {
                                                    if (k == begin) { throw new Exception("Ошибка! Оператор без операнда."); }
                                                    if (k == end) { throw new Exception("Ошибка! Оператор без операнда."); }
                                                    if (calculation_line[k] == "+")
                                                    {
                                                        temp = Convert.ToDouble(calculation_line[k - 1]) + Convert.ToDouble(calculation_line[k + 1]);
                                                        calculation_line.RemoveAt(k - 1);
                                                        calculation_line[k - 1] = temp.ToString();
                                                        calculation_line.RemoveAt(k);
                                                        k = begin;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        temp = Convert.ToDouble(calculation_line[k - 1]) - Convert.ToDouble(calculation_line[k + 1]);
                                                        calculation_line.RemoveAt(k - 1);
                                                        calculation_line[k - 1] = temp.ToString();
                                                        calculation_line.RemoveAt(k);
                                                        k = begin;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //Перевод итогового числа (при необходимости) в научную нотацию и вывод его на экран
                        t = (Convert.ToDouble(calculation_line[0]));
                    }
                    if (calculation_line[0].Contains(","))
                    {
                        int CountNull = 0;
                        for (int i = 0; i < calculation_line[0].Length; i++){
                            if (calculation_line[0][i] == ',')
                            {
                                for(int j = i + 1; j < calculation_line[0].Length; j++)
                                {
                                    if (calculation_line[0][j] == '0')
                                    {
                                        CountNull++;
                                    }
                                    else { break; }
                                }
                                if(CountNull > 4) {
                                    Display.Content = t.ToString("E2");
                                    calculation_line[0] = Convert.ToString(Display.Content);
                                }
                                else {
                                    calculation_line[0] = Convert.ToString(Math.Round(Convert.ToDouble(calculation_line[0]), 7));
                                    Display.Content = calculation_line[0]; 
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (calculation_line[0].Length > 6)
                        {
                            Display.Content = t.ToString("E2");
                            calculation_line[0] = Convert.ToString(Display.Content);
                        }
                        else { Display.Content = calculation_line[0]; }
                    }

                }//Обработка математических ошибок
                catch (Exception ex) {
                    //Очитска старого списка и присвоение ему его прошлой копии
                    calculation_line.Clear();
                    calculation_line.AddRange(CopyList);

                    DisplayError.FontSize = 20;
                    DisplayError.Visibility = Visibility.Visible;
                    Display.Visibility = Visibility.Hidden;
                    DisplayError.Content = ex.Message;
                }
            }
            else if (str == "÷") {//Деление
                if (calculation_line.Count > 0) {
                    if (calculation_line[calculation_line.Count - 1] != "*" && calculation_line[calculation_line.Count - 1] != "/" && calculation_line[calculation_line.Count - 1] != "+" && calculation_line[calculation_line.Count - 1] != "-")
                    {
                        Display.Content += "/";
                        calculation_line.Add("/");
                    }
                }
                else
                {
                    calculation_line.Add("0");
                    Display.Content += "0/";
                    calculation_line.Add("/");
                }
            }
            else if (str == "×") {//Умножение
                if(calculation_line.Count > 0)
                {
                    if (calculation_line[calculation_line.Count - 1] != "*" && calculation_line[calculation_line.Count - 1] != "/" && calculation_line[calculation_line.Count - 1] != "+" && calculation_line[calculation_line.Count - 1] != "-")
                    {
                        Display.Content += "×";
                        calculation_line.Add("*");
                    }
                }
                else
                {
                    calculation_line.Add("0");
                    Display.Content += "0×";
                    calculation_line.Add("*");
                }
            }
            else if (str == "±") {//Смена знака на противоположный
                if ((string)Display.Content != "" && calculation_line.Count == 1)
                {
                    if (Convert.ToString(calculation_line[0][0]) != "-")
                    {
                        Display.Content = "-" + Display.Content;
                    }
                    else
                    {
                        Display.Content = Convert.ToString(Display.Content).Substring(1, Convert.ToString(Display.Content).Length - 1);
                    }
                    if (calculation_line[0].Contains("E")) {
                        if (calculation_line[0][0] == '-')
                        {
                            calculation_line[0] = Convert.ToString(calculation_line[0]).Remove(0, 1);
                        }
                        else
                        {
                            calculation_line[0] = "-" + calculation_line[0];
                        }
                    }
                    else { calculation_line[0] = Convert.ToString(Convert.ToDouble(calculation_line[0]) * (-1)); }
                }
            }
            else if (str == ",") {//Постановка запятой
                if (calculation_line.Count >= 1) {
                    //Если последний символ является запятой или не ялвяется числом, то запятую не ставить
                    if (calculation_line[calculation_line.Count - 1][calculation_line[calculation_line.Count - 1].Length - 1] == ',' || !(int.TryParse(Convert.ToString(calculation_line[calculation_line.Count - 1][calculation_line[calculation_line.Count - 1].Length - 1]), out Null)) || calculation_line[calculation_line.Count-1].Contains(","))
                    {
                        return;
                    }
                    Display.Content += ",";
                    if (calculation_line.Count >= 1)
                    {
                        if (int.TryParse(calculation_line[calculation_line.Count() - 1], out Null))
                        {
                            calculation_line[calculation_line.Count() - 1] = calculation_line[calculation_line.Count() - 1] + str;
                        }
                    }
                }
                else
                {
                    calculation_line.Add("0,");
                    Display.Content += "0,";
                }
            }
            else if (str == "") {//Стереть последний символ
                if ((string)Display.Content != "")
                {
                    if(Convert.ToInt32(calculation_line[calculation_line.Count() - 1].Length) != 1)
                    {
                        //Удаление целого элемента списка в случае sin, cos, tg, ln, sqrt
                        if(((string)calculation_line[calculation_line.Count() - 1])=="ln"|| ((string)calculation_line[calculation_line.Count() - 1]) == "sin"|| ((string)calculation_line[calculation_line.Count() - 1]) == "cos"|| ((string)calculation_line[calculation_line.Count() - 1]) == "tg"|| ((string)calculation_line[calculation_line.Count() - 1]) == "sqrt")
                        {
                            Display.Content = ((string)Display.Content).Substring(0, ((string)Display.Content).Length - calculation_line[calculation_line.Count - 1].Length);
                            calculation_line.RemoveAt(calculation_line.Count() - 1);
                            return;
                        }
                        calculation_line[calculation_line.Count() - 1] = calculation_line[calculation_line.Count() - 1].Substring(0, calculation_line[calculation_line.Count() - 1].Length - 1);
                    }
                    else { calculation_line.RemoveAt(calculation_line.Count() - 1); }
                    Display.Content = ((string)Display.Content).Substring(0, ((string)Display.Content).Length - 1);
                }
            }
            else if (str == "√" || str == "ln" || str == "sin" || str == "cos" || str == "tg") {
                string tempSimbol = "";
                if(calculation_line.Count != 0) { tempSimbol = calculation_line[calculation_line.Count - 1]; }
                if (str == "√" ) {//Квадратный корень
                    if ((string)Display.Content != "")
                    {
                        if (int.TryParse(Convert.ToString(calculation_line[calculation_line.Count() - 1][calculation_line[calculation_line.Count() - 1].Length - 1]), out Null) || calculation_line[calculation_line.Count() - 1][Convert.ToInt32(calculation_line[calculation_line.Count() - 1].Length - 1)] == ',')
                        {
                            calculation_line.Add("*");
                            calculation_line.Add("sqrt");
                        }
                        else { calculation_line.Add("sqrt"); }
                    }
                    else { calculation_line.Add("sqrt"); }
                    calculation_line.Add("(");
                    Display.Content += "√(";
                }
                else if (str == "sin") {//Синус
                    if ((string)Display.Content != "")
                    {
                        if (int.TryParse(Convert.ToString(calculation_line[calculation_line.Count() - 1][calculation_line[calculation_line.Count() - 1].Length - 1]), out Null) || calculation_line[calculation_line.Count() - 1][Convert.ToInt32(calculation_line[calculation_line.Count() - 1].Length - 1)] == ',')
                        {
                            calculation_line.Add("*");
                            calculation_line.Add("sin");
                        }
                        else { calculation_line.Add("sin"); }
                    }
                    else { calculation_line.Add("sin"); }
                    calculation_line.Add("(");
                    Display.Content += "sin(";
                }
                else if (str == "cos") {//Косинус
                    if ((string)Display.Content != "")
                    {
                        if (int.TryParse(Convert.ToString(calculation_line[calculation_line.Count() - 1][calculation_line[calculation_line.Count() - 1].Length - 1]), out Null) || calculation_line[calculation_line.Count() - 1][Convert.ToInt32(calculation_line[calculation_line.Count() - 1].Length - 1)] == ',')
                        {
                            calculation_line.Add("*");
                            calculation_line.Add("cos");
                        }
                        else { calculation_line.Add("cos"); }
                    }
                    else { calculation_line.Add("cos"); }
                    calculation_line.Add("(");
                    Display.Content += "cos(";
                }
                else if (str == "tg") {//Тангенс
                    if ((string)Display.Content != "")
                    {
                        //Обработка случая когда перед тангенсом нужно поставить знак умноженя
                        if (int.TryParse(Convert.ToString(calculation_line[calculation_line.Count() - 1][calculation_line[calculation_line.Count() - 1].Length - 1]), out Null) || calculation_line[calculation_line.Count() - 1][Convert.ToInt32(calculation_line[calculation_line.Count() - 1].Length - 1)] == ',')
                        {
                            calculation_line.Add("*");
                            calculation_line.Add("tg");
                        }
                        else { calculation_line.Add("tg"); }
                    }
                    else { calculation_line.Add("tg"); }
                    calculation_line.Add("(");
                    Display.Content += "tg(";
                }
                else if (str == "ln") {//Натуральный логарифм
                    if ((string)Display.Content != "")
                    {
                        //Обработка случая когда перед логарифмом нужно поставить знак умноженя
                        if (int.TryParse(Convert.ToString(calculation_line[calculation_line.Count() - 1][calculation_line[calculation_line.Count() - 1].Length - 1]), out Null) || calculation_line[calculation_line.Count() - 1][Convert.ToInt32(calculation_line[calculation_line.Count() - 1].Length - 1)] == ',')
                        {
                            calculation_line.Add("*");
                            calculation_line.Add("ln");
                        }
                        else { calculation_line.Add("ln"); }
                    }
                    else { calculation_line.Add("ln"); }
                    calculation_line.Add("(");
                    Display.Content += "ln(";
                }
            }
            else if (str == "(" || str == ")") {//Круглые скобки
                if(str == "(") {
                    //Обработка случая когда перед скобкой нужно поставить знак умноженя
                    if (calculation_line.Count > 0) {
                        if (int.TryParse(Convert.ToString(calculation_line[calculation_line.Count() - 1]), out Null) || calculation_line[calculation_line.Count-1].Contains("E"))
                        {
                            calculation_line.Add("*"); Display.Content += "×";
                        }
                    }
                    calculation_line.Add("("); Display.Content += "(";
                }
                else {
                    calculation_line.Add(")"); Display.Content += ")"; 
                }
            }
            else if (str == "E")//Символ E для научной нотации
            {
                if(calculation_line.Count > 0)
                {
                    if(calculation_line[calculation_line.Count - 1].Contains("E")) { return; }
                    if (double.TryParse(calculation_line[calculation_line.Count-1], out NullD))
                    {
                        Display.Content += "E";
                        calculation_line[calculation_line.Count - 1] = calculation_line[calculation_line.Count - 1] + str;
                    }
                }
            }
            else {//Простые числа и знаки "+" и "-"
                if (calculation_line.Count == 0) { 
                    calculation_line.Clear(); 
                    calculation_line.Add(str);
                    Display.Content = str;
                    return; }
                //Проверка на подряд идущий "+" или "-"
                if (str == "+" && (calculation_line[calculation_line.Count - 1] == "+" || calculation_line[calculation_line.Count - 1] == "*" || calculation_line[calculation_line.Count - 1] == "/" || calculation_line[calculation_line.Count - 1] == "-"))
                {
                    return;
                }
                if (str == "-" && (calculation_line[calculation_line.Count - 1] == "+" || calculation_line[calculation_line.Count - 1] == "*" || calculation_line[calculation_line.Count - 1] == "/" || calculation_line[calculation_line.Count - 1] == "-"))
                {
                    return;
                }
                /*Ввод числа в научной нотации*/
                int Len = (calculation_line[calculation_line.Count - 1]).Length;
                //Ограничение на то, что после E может идти только + или -
                if (Len > 1) {
                    if (calculation_line[calculation_line.Count - 1][Len-1]=='E'&&(str != "+" && str != "-")) { return; }
                }
                if (Len > 2)
                {
                    if (calculation_line[calculation_line.Count - 1][Len - 2] == 'E' && !(int.TryParse(str,out Null))) { return; }
                }
                if ((calculation_line[calculation_line.Count - 1].Contains('E') && (int.TryParse(str,out Null))) || ((str == "+" || str == "-") && calculation_line[calculation_line.Count - 1][Len-1]=='E'))
                {
                    calculation_line[calculation_line.Count - 1] = calculation_line[calculation_line.Count - 1] + str;
                    Display.Content += str;
                    return;
                }
                Display.Content += str;

                //Если предыдущая ячейка содержит число или первый в строке минус, то добавление текущего числа происходит в ту же ячейку
                if ((int.TryParse(Convert.ToString(calculation_line[calculation_line.Count() - 1][calculation_line[calculation_line.Count() - 1].Length - 1]), out Null) || calculation_line[calculation_line.Count() - 1][Convert.ToInt32(calculation_line[calculation_line.Count() - 1].Length - 1)] == ',' || (calculation_line[calculation_line.Count() - 1] == "-") && calculation_line.Count() == 1) && str != "+" && str != "-" && str != "√")
                {
                    calculation_line[calculation_line.Count() - 1] = calculation_line[calculation_line.Count() - 1] + str;
                }
                else if (calculation_line.Count > 2)
                {
                    //Если предыдущая ячейка содержит минус, а предпредыдущая скобку, то добавление текущего числа происходит в ту же ячейку
                    if (calculation_line[calculation_line.Count - 1] == "-" && calculation_line[calculation_line.Count - 2] == "(") {
                        calculation_line[calculation_line.Count() - 1] = calculation_line[calculation_line.Count() - 1] + str;
                    }
                    else { calculation_line.Add(str); }
                }
                else { calculation_line.Add(str); }
            }
            if ((string)Display.Content == "") { Display.Content = "0"; }

        }
        //Кнопка закрытия окна
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        //Перетаскивание окна с помощью панели
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    this.DragMove(); // Перемещение главного окна
                }
            }
            catch { ; }
        }
    }
}
