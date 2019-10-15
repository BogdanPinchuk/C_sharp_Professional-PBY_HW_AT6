using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            }
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

            // назва збірки
            /*dynamic atr = assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute));
            TreeNode node = new TreeNode($"{atr.Title}");
            tree.Nodes.Add(node);*/

            // створення нодів/вузлів
            TreeNode[] nodes = new TreeNode[types.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new TreeNode($"{types[i].Name}");
            }
            tree.Nodes.AddRange(nodes);
        }

        /// <summary>
        /// При виборі якогось із вузлів
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tree_Click(object sender, EventArgs e)
        {
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

            // отримання конструкторів
            if (type.GetConstructors().Length > 0)
            {
                tbInfo.Text += $"Конструктори:" + Environment.NewLine;
                foreach (ConstructorInfo i in type.GetConstructors())
                {
                    tbInfo.Text += $"\t{i.Name}" + Environment.NewLine;
                }
            }

        }
    }
}
