using System;
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

using Microsoft.Windows.Controls;
using System.Data;
using System.IO;

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls.DataVisualization.Charting.Primitives;
using System.Windows.Markup;

namespace PlayingWithFilesKR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        // string helper = null;
        #region Member Variables
        string line;
        string number = null;
        int angle = 0;
        int timestamp = 0;

        string StringName = "Texter";
        ObservableCollection<KeyValuePair<int, int>> valueListOne = new ObservableCollection<KeyValuePair<int, int>>();

        #endregion Member Variables

        public MainWindow()
        {
            InitializeComponent();
            CreateFile();
        }

        //Skapar fil som sedan läses in. Används som test
        private void CreateFile()
        {
            if (!File.Exists(StringName + ".txt"))
            {
                StreamWriter file = new StreamWriter(StringName + ".txt");
                file.WriteLine("45:" + "1" + ";" + "46:" + "2" + ";" + "47:" + "3" + ";" + "10:" + "4" + ";");

                file.Close();
                ReadFromFile();
            }
            else
            {
                using (StreamWriter file = new StreamWriter(StringName + ".txt", true))
                {
                    file.Close();
                    ReadFromFile();
                }
            }
        }

        //En textfil för varje led. vinkel + count på varje rad. Borde gå att hämta de siffrorna och spara i lokala variabler som läggs till i valuelist för plottningen.
        //någonting som gör att man kan koppla textfilerna till den aktuella körningen. datumstämplar? Hur?

        private void ReadFromFile()
        {
            System.IO.StreamReader file = new System.IO.StreamReader(StringName + ".txt");

            while ((line = file.ReadLine()) != null)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    //Går igenom strängen. Lägger till i number tom. ett semikolon hittas.
                    //Lägger in objekt i number allt eftersom strängen gås igenom.
                    if (Convert.ToString(line[i]) != ":" && Convert.ToString(line[i]) != ";") 
                    {
                        number += line[i];
                    }
                        //När det finns ett ":" läggs strängen med siffror in i angle efter det gjorts om till int
                    else if (Convert.ToString(line[i]) == ":") //Första variabeln
                    {
                        angle = Convert.ToInt32(number);
                        number = null;
                    }
                        //Samma fast andra variablen. Lägger också till angle och timestamp i valueListOne
                    else if (Convert.ToString(line[i]) == ";") //den andra variabeln
                    {
                        timestamp = Convert.ToInt32(number);
                        number = null;
                        valueListOne.Add(new KeyValuePair<int, int>(timestamp, angle));
                    }
                    else
                    {
                        //Borde vara felhantering istället - någon form av exeption
                        MessageBox.Show("Något fel har inträffat");
                    }
                }
            }

            var valueList = new ObservableCollection<ObservableCollection<KeyValuePair<int, int>>>();
            valueList.Add(valueListOne);

            //Lägger till i lineChart som sedan plottas
            lineChart.DataContext = valueList;
            file.Close();

        }
    }
}
