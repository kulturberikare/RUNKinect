//Färdig. 
//Funktioner som läser in värden från textfil och plottar sedan den
//Charting toolkit behövs.

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
using System.IO;

namespace ReadFromFileKR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        #region memberVariables
        string line;
        string number = null;
        int angle = 0;
        int timestamp = 0;
        List<KeyValuePair<int, int>> valueList = new List<KeyValuePair<int, int>>();
        #endregion memberVariables

        #region constructor
        public MainWindow()
        {
            InitializeComponent();
            //CreateFile();
            ReadFromFile("RightKnee"); //Såhär kallas funktionen med det aktuella filnamnet.
        }
        #endregion construction

        #region methods
        //Läser in värdena som finns i filen och plottar sedan mha showLineSeries
        private void ReadFromFile(string JointName)
        {
            try
            {
                if (File.Exists(JointName + ".txt"))
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(JointName + ".txt");

                    while ((line = file.ReadLine()) != null)
                    {
                        for (int i = 0; i < line.Length; i++)
                        {
                            //Lägger in objekt i number allt eftersom strängen gås igenom. Slutar när ; eller : påträffas
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
                            //Samma fast andra variablen. Lägger också till angle och timestamp i valueList
                            else if (Convert.ToString(line[i]) == ";") //den andra variabeln
                            {
                                timestamp = Convert.ToInt32(number);
                                number = null;
                                showLineSeries();
                            }
                            else
                            {
                                //Borde vara felhantering istället - någon form av exeption
                                MessageBox.Show("Något fel har inträffat");
                            }
                        }
                    }
                    file.Close();
                }
                else
                {
                    MessageBox.Show("Filen finns inte");
                }
            }

            catch
            {
                //Borde kanske vara något annat vettigare.
                MessageBox.Show("FEL");
            }
        }

        //Lägger till timestamp och angle i valueList som sedan ritas.
        private void showLineSeries()
        {
            valueList.Add(new KeyValuePair<int, int>(timestamp, angle));
            lineChart.DataContext = valueList;
        }
        #endregion methods
    }
}
