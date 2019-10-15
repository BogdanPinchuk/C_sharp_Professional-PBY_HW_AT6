using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LesApp0
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Шлях до збірки
        /// </summary>
        private string path = string.Empty;
        /// <summary>
        /// Збірка
        /// </summary>
        private Assembly assembly = null;

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Відкриття файлу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFD.ShowDialog() == DialogResult.OK)
            {
                // присвоєння шляху/повної назви файла
                path = openFD.FileName;

                if (!(Path.GetExtension(path) == ".exe" ||
                    Path.GetExtension(path) == ".dll"))
                {
                    // якщо завантажено файл але не відповідного розширення
                    toolLb.Text = "File loaded unsuccessfully.";
                    path = string.Empty;
                    analysis.Enabled = false;

                    path = string.Empty;
                    assembly = null;
                    tree.Nodes.Clear();
                    tbInfo.Text = string.Empty;
                    return;
                }

                // якщо завантажено файл з правильним розширенням
                toolLb.Text = $"File loaded: {Path.GetFileName(path)}";
                analysis.Enabled = true;
            }
            else
            {
                // якщо натиснута відміна
                toolLb.Text = "File wasn't loaded.";
                analysis.Enabled = false;
                path = string.Empty;
                assembly = null;
                tree.Nodes.Clear();
                tbInfo.Text = string.Empty;

            }

            // додаткова умова ДЗ
            DetectHWLib();
        }

        /// <summary>
        /// Завантаження збірки
        /// </summary>
        private void LoadLib()
        {
            try
            {
                assembly = Assembly.LoadFrom(path);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        /// <summary>
        /// Аналіз файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void analysis_Click(object sender, EventArgs e)
        {
            if (path == null || path == string.Empty)
                return;

            // завантаження збірки
            LoadLib();

            // виведення інформації про всі типи у збірці
            Type[] types = assembly.GetTypes();

            // Очищення вузлів
            tree.Nodes.Clear();

            // створення нодів/вузлів
            TreeNode[] nodes = new TreeNode[types.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new TreeNode($"{types[i].Name}");
            }
            tree.Nodes.AddRange(nodes);

            // додаткова умова ДЗ
            DetectHWLib();
        }

        /// <summary>
        /// При виборі якогось із вузлів
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tree_Click(object sender, EventArgs e)
        {
            // зміна курсора
            this.Cursor = Cursors.AppStarting;

            // Аналіз вибраного вузла
            TreeNode tn = tree.GetNodeAt(tree.PointToClient(MousePosition));

            // Очищення інформаційного вікна
            tbInfo.Text = string.Empty;

            // вибраний тип
            Type type = null;

            // вибір вказаного типу
            foreach (Type i in assembly.GetTypes())
            {
                if (i.Name == tn.Text)
                {
                    type = i;
                    break;
                }
            }

            tbInfo.Text += $"Тип: {type?.Name}" + Environment.NewLine;

            #region Класифікація типу
            tbInfo.Text += $"Класифікація типу:";
            if (type.IsAbstract) tbInfo.Text += $" абстрактний,";
            if (type.IsArray) tbInfo.Text += $" масив,";
            if (type.IsClass) tbInfo.Text += $" клас,";
            if (type.IsEnum) tbInfo.Text += $" перелік,";
            if (type.IsInterface) tbInfo.Text += $" інтерфейс,";
            if (type.IsNested) tbInfo.Text += $" вкладений тип,";
            if (type.IsSealed) tbInfo.Text += $" закритий від наслідування,";
            if (type.IsValueType) tbInfo.Text += $" структура,";
            tbInfo.Text = tbInfo.Text.TrimEnd(',') + ";" + Environment.NewLine + Environment.NewLine;
            #endregion

            // отримання полів
            if (type.GetFields(BindingFlags.Instance | 
                BindingFlags.Static | BindingFlags.Public | 
                BindingFlags.NonPublic).Length > 0)
            {
                tbInfo.Text += $"Поля:" + Environment.NewLine;
                foreach (FieldInfo i in type.GetFields(BindingFlags.Instance |
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic))
                {
                    tbInfo.Text += $"\t{i.Name}" + Environment.NewLine;
                }
            }

            // отримання властивостей
            if (type.GetProperties(BindingFlags.Static | BindingFlags.Instance |
                BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.DeclaredOnly).Length > 0)
            {
                tbInfo.Text += $"Властивості:" + Environment.NewLine;
                foreach (PropertyInfo i in type.GetProperties(BindingFlags.Static | 
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                    BindingFlags.DeclaredOnly))
                {
                    tbInfo.Text += $"\t{i.Name}" + Environment.NewLine;
                }
            }

            // отримання методів
            if (type.GetMethods(BindingFlags.Static | BindingFlags.Instance |
                BindingFlags.NonPublic | BindingFlags.Public | 
                BindingFlags.DeclaredOnly).Length > 0)
            {
                tbInfo.Text += $"Методи:" + Environment.NewLine;
                foreach (MethodInfo i in type.GetMethods(BindingFlags.Static |
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | 
                    BindingFlags.DeclaredOnly))
                {
                    tbInfo.Text += $"\t{i.Name}" + Environment.NewLine;
                }
            }

            // отримання перелічень
            if (type.IsEnum && type.GetEnumNames().Length > 0)
            {
                tbInfo.Text += $"Перелічення:" + Environment.NewLine;
                foreach (string i in type.GetEnumNames())
                {
                    tbInfo.Text += $"\t{i}" + Environment.NewLine;
                }
            }

            // отримання інтерфейсів
            if (type.GetInterfaces().Length > 0)
            {
                tbInfo.Text += $"Інтерфейси:" + Environment.NewLine;
                foreach (Type i in type.GetInterfaces())
                {
                    tbInfo.Text += $"\t{i.Name}" + Environment.NewLine;
                }
            }

            // отримання подій
            if (type.GetEvents().Length > 0)
            {
                tbInfo.Text += $"Події:" + Environment.NewLine;
                foreach (EventInfo i in type.GetEvents())
                {
                    tbInfo.Text += $"\t{i.Name}" + Environment.NewLine;
                }
            }

            // отримання конструкторів
            if (type.GetConstructors().Length > 0)
            {
                tbInfo.Text += $"Конструктори:" + Environment.NewLine;
                foreach (ConstructorInfo i in type.GetConstructors())
                {
                    tbInfo.Text += $"\t{i.Name}" + Environment.NewLine;
                }
            }

            // отримання вкладені типи
            if (type.GetNestedTypes().Length > 0)
            {
                tbInfo.Text += $"Вкладені типи:" + Environment.NewLine;
                foreach (Type i in type.GetNestedTypes())
                {
                    tbInfo.Text += $"\t{i.Name}" + Environment.NewLine;
                }
            }

            // повернення стандартного виду курсору
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Закриття форми
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
            => this.Close();

        /// <summary>
        /// Детектор файла із домашнього завдання 
        /// </summary>
        private void DetectHWLib()
        {
            if (assembly == null)
            {
                hw.Visible = false;
                return;
            }

            // перевірка назви збірки
            if (assembly.GetTypes()[0].Namespace != "ReflectionCW")
            {
                hw.Visible = false;
                return;
            }

            // вмикаємо доступ до схованого меню
            hw.Visible = true;
        }

        /// <summary>
        /// Запуск схованої реалізації
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hw_Click(object sender, EventArgs e)
        {
            // http://www.cyberforum.ru/windows-forms/thread220529.html

            // запуск по слабклій ссилці в іншому потоці
            new Thread(NewConsole).Start();
        }

        /// <summary>
        /// Запуск консолі з під графічного інтерфейсу
        /// </summary>
        private void NewConsole()
        {
            if (AllocConsole())
            {
                // join unicode
                Console.OutputEncoding = Encoding.Unicode;

                Console.WriteLine("Створення екземплярів всіх класів і запуск всіх ментодів без змін.");

                // список типів класів
                List<Type> types = new List<Type>();

                // фільтр по класам
                foreach (Type i in assembly.GetTypes())
                {
                    if (i.IsClass)
                    {
                        types.Add(i);
                    }
                }

                // виведення інформації
                Console.WriteLine("Найдені класи: ");
                foreach (Type i in types)
                {
                    Console.WriteLine($"\tName: {i.Name}");
                }

                // створення екземплярів всік класів
                List<object> classes = new List<object>();
                foreach (Type i in types)
                {
                    classes.Add(Activator.CreateInstance(i));
                }

                // перебір і запуск всіх методів
                Console.WriteLine("Запуск на виконання всіх методів без внесення змін.");
                foreach (object i in classes)
                {
                    MethodInfo[] methods = i.GetType()
                        .GetMethods(BindingFlags.Instance |
                        BindingFlags.Static|
                        BindingFlags.NonPublic| 
                        BindingFlags.Public|
                        BindingFlags.DeclaredOnly);

                    foreach (MethodInfo j in methods)
                    {
                        j.Invoke(i, null);
                    }
                }

                Console.ReadKey(true);
                FreeConsole();
            }
        }

        /// <summary>
        /// Запуск консолі
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        /// <summary>
        /// Закриття консолі
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

    }
}
