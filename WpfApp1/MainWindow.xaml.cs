using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Resources.ResXFileRef;
using System.Diagnostics;
using static iRestoUniVerse.MainWindow;
using System.IO;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Security.Policy;
using FluentFTP;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Threading;

namespace iRestoUniVerse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ObservableCollection<IikoVersions> iikoVerList { get; set; }
        public ObservableCollection<Organizations> organizationList { get; set; }
        //Работа с XML
        XDocument xdocOrg = new XDocument(); //документ с организациями
        XDocument xdocBackVer = new XDocument(); // документ с версиями Office
        XDocument xBackConfig = new XDocument();
        //элементы версий
        XElement iikoVersionsXML;
        XAttribute verName;
        XAttribute verType;
        XElement pathtoIiko;
        //элементы организаций
        XElement organizationsXML;
        XAttribute orgName;
        XElement iikoVerOrgStart;
        XElement orgProtocol;
        XElement orgPort;
        XElement adressOrg;
        XElement commentOrg;
        XElement iikoDistrDirX;

        public MainWindow()
        {
            InitializeComponent();
            UpdateXML();
            LoadXML();
            iikoVersionsOut();
            organizationOut();
            FTPPartners();
        }
        public void UpdateXML()
        {
            if (File.Exists(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml"))
            {
                xdocOrg = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");
                // получаем корневой узел
                organizationsXML = xdocOrg.Element("organizations");
                foreach (var org in organizationsXML.Elements("org"))
                {
                    if (org.Element("comments") == null)
                    {
                        org.Add(new XElement("comments"));
                      
                    } else
                    if (org.Element("tmpFolderOrg") == null)
                    {
                        org.Add(new XElement("tmpFolderOrg", Converter.ConvertToLatin(org.Attribute("name").Value.ToString())));
                    }
                }
               
                xdocOrg.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");
            }

        }
        public void LoadXML()
        {
            
            if (!File.Exists(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml"))
            {
                XDocument newDocOrg = new XDocument();
                newDocOrg.Add(new XElement("organizations"));
                newDocOrg.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");
                System.Windows.MessageBox.Show("Создаю");
            }
            if (!File.Exists(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configFTPIiko.xml"))
            {
                XDocument newDocFTP = new XDocument();
                newDocFTP.Add(new XElement("iikoFTP"));
                XElement iikoFTPX = newDocFTP.Element("iikoFTP");
                iikoFTPX.Add(new XElement("distrDir"),
                    new XElement("portableDir"));
                newDocFTP.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configFTPIiko.xml");
            }
            if (!File.Exists(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configIikoVersion.xml"))
            {
                XDocument newVerOrg = new XDocument();
                newVerOrg.Add(new XElement("iikoVersions"));
                newVerOrg.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configIikoVersion.xml");
            }
            //загружаем документ с версиями BackOffice
            xdocBackVer = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configIikoVersion.xml");
            iikoVersionsXML = xdocBackVer.Element("iikoVersions");
            //загружаем документ с организациями
            xdocOrg = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");
            // получаем корневой узел
            organizationsXML = xdocOrg.Element("organizations");
            XDocument xdocFTP = new XDocument();
            xdocFTP = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configFTPIiko.xml");
            XElement iikoFTP = xdocFTP.Element("iikoFTP");
            XElement distrDir = iikoFTP.Element("distrDir");
            if (distrDir != null)
            {
                iikoDistrDirBox.Text = distrDir.Value;
            }
           

        }
        public void iikoVersionsOut()
        {
            iikoVerList = new ObservableCollection<IikoVersions>();
            iikoVerOut.ItemsSource = iikoVerList;
            if (iikoVersionsXML != null)
            {
                // проходим по всем элементам iikoVer
                foreach (var iikoVer in iikoVersionsXML.Elements("iikoVer"))
                {
                    verName = iikoVer.Attribute("name");
                    verType = iikoVer.Attribute("iiko");
                    pathtoIiko = iikoVer.Element("pathtoIiko");
                    IikoVersions iikoVersion = new IikoVersions
                    {
                        name = verName.Value,
                        iikoPath = pathtoIiko.Value,
                    };

                    iikoVerList.Add(iikoVersion);

                }


                for (int i = 0; i < iikoVerList.Count; i++)
                {

                    if (!iikoVersionStart.Items.Contains(iikoVerList[i].name.ToString()))
                    {
                        iikoVersionStart.Items.Add(iikoVerList[i].name.ToString());
                    }

                }

                iikoVersionStart.SelectedIndex = 0;
                iikoVerOut.ItemsSource = iikoVerList;

            }
        }
        public void removeVer_Click(object sender, EventArgs e)
        {
            System.Windows.Controls.Button tag = sender as System.Windows.Controls.Button;
            if (iikoVersionsXML != null)
            {
                //получим элемент iikoVer с name = rk.xe
                var iikoVer = iikoVersionsXML.Elements("iikoVer")
                    .FirstOrDefault(p => p.Attribute("name")?.Value == tag.Tag.ToString());
                // и удалим его
                if (iikoVer != null)
                {

                    iikoVer.Remove();
                    if (iikoVersionStart.Items.Contains(iikoVer.Attribute("name").Value.ToString()))
                    {
                        iikoVersionStart.Items.Remove(iikoVer.Attribute("name").Value.ToString());
                    }

                }


                xdocBackVer.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configIikoVersion.xml");
                iikoVersionsOut();
            }
        }
        public void viewIikoFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            ;
            var result = FBD.ShowDialog();
            addIikoPathBox.Text = (FBD.SelectedPath);
            if (!File.Exists(FBD.SelectedPath + "/BackOffice.exe")) {
                System.Windows.MessageBox.Show("Не найден файл запуска BackOffice.exe, дружище!");

            }
            if (File.Exists(FBD.SelectedPath + "/BackOffice.exe"))
            {
                string iikoVersion = FileVersionInfo.GetVersionInfo(FBD.SelectedPath + "/BackOffice.exe").FileVersion.ToString();
                addIikoVersionBox.Text = iikoVersion;
            }

            if (File.Exists(FBD.SelectedPath + "/ChainSessions.dll")) {
                typeIikoBox.SelectedIndex = 1;
            }
            else
            {
                typeIikoBox.SelectedIndex = 0;
            }



        }
        public void viewIikoDistr_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            
            var result = FBD.ShowDialog();
            iikoDistrDirBox.Text = (FBD.SelectedPath);
            XDocument xdocFTP = new XDocument();
              xdocFTP = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configFTPIiko.xml");
            XElement iikoFTP = xdocFTP.Element("iikoFTP");
            XElement distrDir = iikoFTP.Element("distrDir");
            distrDir.Value = iikoDistrDirBox.Text;
            xdocFTP.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configFTPIiko.xml");


        }
        public void openIikoDistr_Click(object sender, EventArgs e)
        {
            XDocument xdocFTP = new XDocument();
            xdocFTP = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configFTPIiko.xml");
            XElement iikoFTP = xdocFTP.Element("iikoFTP");
            XElement distrDir = iikoFTP.Element("distrDir");
            Process.Start("explorer.exe", distrDir.Value.ToString());


        }

        public void createVersion_Click(object sender, EventArgs e)
        {
            int duplicate = 0;
            if (iikoVersionsXML != null)
            {
                for (int i = 0; i < iikoVerList.Count; i++)
                {
                    if (iikoVerList[i].name == addIikoVersionBox.Text + " " + typeIikoBox.Text)
                    {
                        System.Windows.Forms.MessageBox.Show("Такая версия уже есть!");
                        duplicate += 1;
                    }
                    else
                    {

                    }
                }

                if (duplicate == 0)
                {
                    string pathFile = addIikoPathBox.Text + "/BackOffice.exe";
                    if (!System.IO.File.Exists(pathFile))
                    {
                        System.Windows.Forms.MessageBox.Show("Не найден файл запуска BackOffice.exe, дружище!");
                    }
                    else
                    {

                        // добавляем новый элемент
                        iikoVersionsXML.Add(new XElement("iikoVer",
                                    new XAttribute("name", addIikoVersionBox.Text + " " + typeIikoBox.Text),
                                    new XElement("pathtoIiko", addIikoPathBox.Text)));
                    }
                }

            }
            xdocBackVer.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configIikoVersion.xml");
            iikoVersionsOut();
        }


        private void FindVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Find.Text != null)
            {
                iikoVerList = new ObservableCollection<IikoVersions>();
                if (iikoVersionsXML != null)
                {
                    // проходим по всем элементам iikoVer
                    foreach (var iikoVer in iikoVersionsXML.Elements("iikoVer"))
                    {

                        verName = iikoVer.Attribute("name");
                        verType = iikoVer.Attribute("iiko");
                        pathtoIiko = iikoVer.Element("pathtoIiko");
                        string a = verName.Value.ToString().ToLower();
                        string b = Find.Text.ToString().ToLower();
                        if (a.Contains(b))
                        {
                            IikoVersions iikoVersion = new IikoVersions
                            {
                                name = verName.Value,
                                iikoPath = pathtoIiko.Value,
                            };
                            iikoVerList.Add(iikoVersion);
                        }

                        iikoVerOut.ItemsSource = iikoVerList;
                    }

                }
            }

        }
        //блок оранизаций
        public void organizationOut()
        {
            organizationList = new ObservableCollection<Organizations>();
            orgOut.ItemsSource = organizationList;
            if (organizationsXML != null)
            {
                // проходим по всем элементам iikoVer
                foreach (var org in organizationsXML.Elements("org"))
                {
                    orgName = org.Attribute("name");
                    iikoVerOrgStart = org.Element("iikoVer");
                    adressOrg = org.Element("adress");
                    orgProtocol = org.Element("protocol");
                    orgPort = org.Element("port");
                    commentOrg = org.Element("comments");


                    Organizations orgList = new Organizations
                    {
                        organizationName = orgName.Value,
                        iikoVersionStart = iikoVerOrgStart.Value,
                        comments = commentOrg.Value,
                        adress = adressOrg.Value,
                        port = orgPort.Value
                    };

                    organizationList.Add(orgList);

                }


                orgOut.ItemsSource = organizationList;
            }
        }
        void iikoVerOrg_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Windows.Controls.ComboBox tag = sender as System.Windows.Controls.ComboBox;
            if (organizations != null)
            {

                // проходим по всем элементам iikoVer
                foreach (XElement org in organizationsXML.Elements("org"))
                {
                    XAttribute orgNameXML = org.Attribute("name");

                    if (tag.Tag.ToString() == orgNameXML.Value)
                    {
                        XElement iikoVerOrgStart = org.Element("iikoVer");
                        if (tag.SelectedItem != null)
                        {
                            iikoVerOrgStart.Value = tag.SelectedItem.ToString();
                        }
                        else
                        {
                            tag.SelectedItem = iikoVersionStart.SelectedItem;
                            iikoVerOrgStart.Value = tag.SelectedItem.ToString();
                        }
                        xdocOrg.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");
                    }
                }
            }

        }
        private void openOrgan_Click(object sender, EventArgs e)
        {

            System.Windows.Controls.Button tag = sender as System.Windows.Controls.Button;
            if (organizations != null)
            {

                // проходим по всем элементам iikoVer
                foreach (var org in organizationsXML.Elements("org"))
                {
                    XAttribute orgNameXML = org.Attribute("name");

                    if (tag.Tag.ToString() == orgNameXML.Value)
                    {
                        XElement iikoVerOrgStart = org.Element("iikoVer");
                        foreach (XElement iikoVerXML in iikoVersionsXML.Elements("iikoVer"))
                        {
                            XAttribute iikoVerName = iikoVerXML.Attribute("name");
                            XElement iikoPathXML = iikoVerXML.Element("pathtoIiko");
                            bool isChain;
                            if (!org.Element("iikoVer").Value.Contains("Chain"))
                            {
                                isChain = false;
                            }
                            else
                            {
                                isChain = true;
                            }
                            if (iikoVerName.Value == iikoVerOrgStart.Value)
                            {

                                OpenIiko(
                                    iikoPathXML.Value + "/BackOffice.exe",
                                    org.Element("tmpFolderOrg").Value,
                                    org.Element("protocol").Value,
                                    org.Element("adress").Value,
                                    org.Element("port").Value,
                                    isChain
                                );
                            }
                        }
                    }
                }
            }
        }
        public void OpenIiko(string pathToIiko, string tmpFolderName, string protocol, string address, string port, bool isChain)
        {
            try
            {
                var pathToAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string pathToTmpFolder = isChain ? System.IO.Path.Combine(pathToAppData, @"iiko\Chain", tmpFolderName)
                    : System.IO.Path.Combine(pathToAppData, @"iiko\Rms", tmpFolderName);
                var configFile = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\backclient.config.xml";
                if (!File.Exists(configFile)) throw new FileNotFoundException($"{configFile} не найден, верните его в папку программы.");
                Directory.CreateDirectory(pathToTmpFolder);
                var pathToConfigFolder = System.IO.Path.Combine(pathToTmpFolder, @"config");
                Directory.CreateDirectory(pathToConfigFolder);
                var fileName = System.IO.Path.GetFileName(configFile);
                var destFile = System.IO.Path.Combine(pathToConfigFolder, fileName);
                if (!Directory.Exists(pathToTmpFolder)) // если уже есть то просто запускаем айку
                {
                    // создаем папки и копируем конфиг файл (он должен лежать в папке проги)

                    File.Copy(configFile, destFile, true);
                    // парсим destFile и заменяем адрес сервера, порт и протокол

                    xBackConfig = XDocument.Load(destFile);
                    XElement config = xBackConfig.Element("config");
                    XElement serverList = config.Element("ServersList");
                    XElement serverAddr = serverList.Element("ServerAddr");
                    XElement Iprotocol = serverList.Element("Protocol");
                    XElement Iport = serverList.Element("Port");
                    XElement edition = serverList.Element("Edition");

                    serverAddr.Value = address;
                    Iprotocol.Value = protocol;
                    Iport.Value = port;
                    if (isChain) edition.Value = "chain"; // для чейна как оказалось нужно еще одно поле поменять
                    xBackConfig.Save(destFile);



                }
                else
                {
                    if (!File.Exists(destFile + "/" + configFile))
                    {
                        File.Copy(configFile, destFile, true);
                    }
                    xBackConfig = XDocument.Load(destFile);
                    XElement config = xBackConfig.Element("config");
                    XElement serverAddr47 = config.Element("ServerAddr");
                    XElement serverPort47 = config.Element("ServerPort");
                    XElement serverList = config.Element("ServersList");
                    XElement serverAddr = serverList.Element("ServerAddr");
                    XElement iikoProtocol = serverList.Element("Protocol");
                    XElement iikoPort = serverList.Element("Port");
                    XElement edition = serverList.Element("Edition");
                    XElement chainAddr47 = config.Element("ChainServerAddr");
                    XElement chainPort47 = config.Element("ChainServerPort");
                    iikoProtocol.Value = protocol.ToLower();
                    if (isChain)
                    {
                        edition.Value = "chain"; ;
                    }// для чейна как оказалось нужно еще одно поле поменять

                    if (serverAddr.Value != address || iikoPort.Value != port)
                    {
                        iikoPort.Value = port;
                        serverAddr.Value = address;
                        chainAddr47.Value = address;
                        chainPort47.Value = port;


                    }
                    xBackConfig.Save(destFile);

                }

                var process = new Process
                {

                    StartInfo =
                {
                  FileName = pathToIiko,
                  Arguments = $"/AdditionalTmpFolder=\"{tmpFolderName}\""
                }
                };
            
            
                process.Start();
            }catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message.ToString());
            }
        }
        private void createOrganization_Click(object sender, EventArgs e)
        {

            int duplicate = 0;
            if (addOrganizationNameBox.Text.Replace(" ", "") == "")
            {
                System.Windows.MessageBox.Show("Введи что-нибудь осмысленное!");
                duplicate += 1;
            }

            if (organizationsXML != null)
            {
                for (int i = 0; i < organizationList.Count; i++)
                {
                    if (organizationList[i].organizationName.ToString() == addOrganizationNameBox.Text)
                    {
                        System.Windows.Forms.MessageBox.Show("Такая организация уже есть!");
                        duplicate += 1;
                    }
                    else
                    {

                    }

                }
                if (duplicate == 0)
                {

                    // добавляем новый элемент
                    organizationsXML.Add(new XElement("org",
                        new XAttribute("name", addOrganizationNameBox.Text),
                        new XElement("iikoVer", iikoVersionStart.Text),
                        new XElement("protocol", addProtocol.Text),
                        new XElement("port", addPortBox.Text),
                        new XElement("adress", addAddresBox.Text),
                        new XElement("tmpFolderOrg", Converter.ConvertToLatin(addOrganizationNameBox.Text)),
                        new XElement("comments", " ")
                        ));

                }




            }
            xdocOrg.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");
            organizationOut();
        }
        private void removeOrg_Click(object sender, EventArgs e)
        {
            System.Windows.Controls.Button tag = sender as System.Windows.Controls.Button;


            if (organizationsXML != null)
            {

                //получим элемент iikoVer с name = rk.xe
                var orgName = organizationsXML.Elements("org")
                    .FirstOrDefault(p => p.Attribute("name")?.Value == tag.Tag.ToString());
                // и удалим его
                if (orgName != null)
                {
                    var pathToAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string pathToTmpFolder;
                    if (orgName.Element("iikoVer").ToString().Contains("Chain"))
                    {
                        pathToTmpFolder = System.IO.Path.Combine(pathToAppData, @"iiko\Chain", Converter.ConvertToLatin(orgName.Attribute("name").Value.ToString()));
                        if (Directory.Exists(pathToTmpFolder))
                        {

                            Directory.Delete(pathToTmpFolder, true);
                        }
                    }

                    if (!orgName.Element("iikoVer").ToString().Contains("Chain"))
                    {
                        pathToTmpFolder = System.IO.Path.Combine(pathToAppData, @"iiko\Rms", Converter.ConvertToLatin(orgName.Attribute("name").Value.ToString()));
                        if (Directory.Exists(pathToTmpFolder))
                        {

                            Directory.Delete(pathToTmpFolder, true);
                        }
                    }
                    orgName.Remove();
                    xdocOrg.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");

                }

            }
            organizationOut();
        }
        private void EditOrg_Click(object sender, EventArgs e)
        {
            System.Windows.Controls.Button tag = sender as System.Windows.Controls.Button;
            DependencyObject parentObj = VisualTreeHelper.GetParent(tag);
            int childrenCount = VisualTreeHelper.GetChildrenCount(parentObj);
            string commentsToOrg;
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parentObj, i);
                System.Windows.Controls.TextBox childType = child as System.Windows.Controls.TextBox;
                if (childType != null)
                {
                    if (childType.Name == "orgCommentsBox")
                    {
                        // проходим по всем элементам iikoVer
                        foreach (var org in organizationsXML.Elements("org"))
                        {
                            if (org.Attribute("name").Value == childType.Tag.ToString())
                            {
                                XElement orgComment = org.Element("comments");
                                orgComment.Value = childType.Text;
                                organizationsXML.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");
                                organizationOut();
                            }

                        }
                    }
                    if (childType.Name == "portOrgBox")
                    {
                        // проходим по всем элементам iikoVer
                        foreach (var org in organizationsXML.Elements("org"))
                        {
                            if (org.Attribute("name").Value == childType.Tag.ToString())
                            {
                                XElement orgPort = org.Element("port");
                                orgPort.Value = childType.Text;
                                organizationsXML.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");
                                organizationOut();
                            }

                        }
                    }
                    if (childType.Name == "orgNameBox")
                    {
                        // проходим по всем элементам iikoVer
                        foreach (var org in organizationsXML.Elements("org"))
                        {
                            if (org.Attribute("name").Value == childType.Tag.ToString())
                            {
                                XAttribute orgName = org.Attribute("name");
                                orgName.Value = childType.Text;
                                organizationsXML.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");
                                organizationOut();
                            }

                        }
                    }
                    if (childType.Name == "adressOrgBox")
                    {
                        // проходим по всем элементам iikoVer
                        foreach (var org in organizationsXML.Elements("org"))
                        {
                            if (org.Attribute("name").Value == childType.Tag.ToString() && org.Element("adress").Value != childType.Text)
                            {
                                var pathToAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                                string pathToTmpFolder;
                                if (org.Element("iikoVer").ToString().Contains("Chain")) {
                                    pathToTmpFolder = System.IO.Path.Combine(pathToAppData, @"iiko\Chain", Converter.ConvertToLatin(org.Attribute("name").Value.ToString()));
                                }
                                else
                                {
                                    pathToTmpFolder = System.IO.Path.Combine(pathToAppData, @"iiko\Rms", Converter.ConvertToLatin(org.Attribute("name").Value.ToString()));
                                }
                                if (Directory.Exists(pathToTmpFolder)) {
                                    Directory.Delete(pathToTmpFolder, true);
                                }
                                XElement adress = org.Element("adress");
                                adress.Value = childType.Text;
                                organizationsXML.Save(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configOrganization.xml");

                                organizationOut();
                            }

                        }
                    }
                }

            }


        }

        private void FindOrganization_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FindOrganization.Text != null)
            {
                organizationList = new ObservableCollection<Organizations>();
                if (organizationsXML != null)
                {
                    // проходим по всем элементам iikoVer
                    foreach (var org in organizationsXML.Elements("org"))
                    {
                        orgName = org.Attribute("name");
                        iikoVerOrgStart = org.Element("iikoVer");
                        adressOrg = org.Element("adress");
                        orgProtocol = org.Element("protocol");
                        orgPort = org.Element("port");
                        commentOrg = org.Element("comments");
                        string a = orgName.Value.ToString().ToLower();
                        string b = FindOrganization.Text.ToString().ToLower();

                        if (a.Contains(b))
                        {

                            Organizations orgList = new Organizations
                            {
                                organizationName = orgName.Value,
                                iikoVersionStart = iikoVerOrgStart.Value,
                                comments = commentOrg.Value,
                                adress = adressOrg.Value,
                                port = orgPort.Value
                            };


                            organizationList.Add(orgList);


                        }

                    }
                    orgOut.ItemsSource = organizationList;
                }

            }
        }
        public void FTPPartners()
        {
            FtpClient iikoFTP = new FtpClient("ftp.iiko.ru", "partners", "partners#iiko");
            iikoFTP.Connect();
            ObservableCollection<iikoVerFTPClass> iikoVerFTPList = new ObservableCollection<iikoVerFTPClass>();
            List<string> iikoFTPTemp = new List<string>();
            var items = iikoFTP.GetListing("/release_iiko/");
            foreach (FtpListItem item in items)
            {
                if (item.Type == FtpObjectType.Directory)
                {
                    if (item.Name.StartsWith("7.") || item.Name.StartsWith("8."))
                    {
                        iikoFTPTemp.Add(item.Name);

                    }

                }

            }
            iikoFTPTemp.Sort();
            for (int i = 0; i < iikoFTPTemp.Count; i++)
            {
                iikoVerFTPClass iikoVerFTPAdd = new iikoVerFTPClass
                {
                    iikoVerFTPNameDir = iikoFTPTemp[i]
                };
                iikoVerFTPList.Add(iikoVerFTPAdd);
            }
            iikoVerFTP.ItemsSource = iikoVerFTPList;

        }
        private void DownloadIikoFTP_Click(object sender, EventArgs e)
        {
            System.Windows.Controls.Button tag = sender as System.Windows.Controls.Button;
            XDocument xdocFTP = new XDocument();
            xdocFTP = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configFTPIiko.xml");
            XElement iikoFTPX = xdocFTP.Element("iikoFTP");
            XElement distrDir = iikoFTPX.Element("distrDir");
            if (distrDir.Value == "")
            {
                System.Windows.MessageBox.Show("Укажи куда качать, дружище!");
            }
            else
            {
                DownloadIikoFTP(tag);
            }
        }
    

        public static async Task DownloadIikoFTP(System.Windows.Controls.Button tag2)
        {


            var token = new CancellationToken();
            using (var iikoFTP = new AsyncFtpClient("ftp.iiko.ru", "partners", "partners#iiko"))
            {
                await iikoFTP.Connect(token);

                // define the progress tracking callback
                Progress<FtpProgress> progress = new Progress<FtpProgress>(p =>
                {
                    if (p.Progress == 1)
                    {
                        // all done!
                    }
                    else
                    {
                        tag2.Content= (((int)p.Progress)).ToString() + "%";
                    }
                });
                XDocument xdocFTP = new XDocument();
                xdocFTP = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configFTPIiko.xml");
                XElement iikoFTPX = xdocFTP.Element("iikoFTP");
                XElement distrDir = iikoFTPX.Element("distrDir");


                if (tag2.Name == "downloadIikoOffice")
                {
                    // download a file with progress tracking
                    await iikoFTP.DownloadFile(distrDir.Value.ToString() + "/" + tag2.Tag.ToString() + "_BackOffice.exe", "/release_iiko/" + tag2.Tag.ToString() + "/Setup/Offline/Setup.RMS.BackOffice.exe", FtpLocalExists.Overwrite, FtpVerify.None, progress, token);
                }
                if (tag2.Name == "downloadIikoChain")
                {
                    // download a file with progress tracking
                    await iikoFTP.DownloadFile(distrDir.Value.ToString() + "/" + tag2.Tag.ToString() + "_ChainOffice.exe", "/release_iiko/" + tag2.Tag.ToString() + "/Setup/Offline/Setup.Chain.BackOffice.exe", FtpLocalExists.Overwrite, FtpVerify.None, progress, token);
                }
                if (tag2.Name == "downloadIikoFront")
                {
                    // download a file with progress tracking
                    await iikoFTP.DownloadFile(distrDir.Value.ToString() + "/" + tag2.Tag.ToString() + "_iikoFront.exe", "/release_iiko/" + tag2.Tag.ToString() + "/Setup/Offline/Setup.Front.exe", FtpLocalExists.Overwrite, FtpVerify.None, progress, token);
                }
            }
        }
           
       
     
    }
}
public class IikoVersions{
    public string name { get; set; }
    public string iikoPath { get; set; }
    
}
public class Organizations
{
    public string organizationName { get; set; }
    public string iikoVersionStart { get; set; }
    public string adress { get; set; }
    public string port { get; set; }
    public string comments { get; set; }
    public string tmpFolderOrg { get; set; }

}
public class iikoVerFTPClass
{
    public string iikoVerFTPNameDir { get; set; }

}
public static class Converter
{
    private static readonly Dictionary<char, string> ConvertedLetters = new Dictionary<char, string>
    {
        {'а', "a"},
        {'б', "b"},
        {'в', "v"},
        {'г', "g"},
        {'д', "d"},
        {'е', "e"},
        {'ё', "yo"},
        {'ж', "zh"},
        {'з', "z"},
        {'и', "i"},
        {'й', "j"},
        {'к', "k"},
        {'л', "l"},
        {'м', "m"},
        {'н', "n"},
        {'о', "o"},
        {'п', "p"},
        {'р', "r"},
        {'с', "s"},
        {'т', "t"},
        {'у', "u"},
        {'ф', "f"},
        {'х', "h"},
        {'ц', "c"},
        {'ч', "ch"},
        {'ш', "sh"},
        {'щ', "sch"},
        {'ъ', "j"},
        {'ы', "i"},
        {'ь', "j"},
        {'э', "e"},
        {'ю', "yu"},
        {'я', "ya"},
        {'А', "A"},
        {'Б', "B"},
        {'В', "V"},
        {'Г', "G"},
        {'Д', "D"},
        {'Е', "E"},
        {'Ё', "Yo"},
        {'Ж', "Zh"},
        {'З', "Z"},
        {'И', "I"},
        {'Й', "J"},
        {'К', "K"},
        {'Л', "L"},
        {'М', "M"},
        {'Н', "N"},
        {'О', "O"},
        {'П', "P"},
        {'Р', "R"},
        {'С', "S"},
        {'Т', "T"},
        {'У', "U"},
        {'Ф', "F"},
        {'Х', "H"},
        {'Ц', "C"},
        {'Ч', "Ch"},
        {'Ш', "Sh"},
        {'Щ', "Sch"},
        {'Ъ', "J"},
        {'Ы', "I"},
        {'Ь', "J"},
        {'Э', "E"},
        {'Ю', "Yu"},
        {'Я', "Ya"},
         {' ', ""},
        {'.', "" },
        {'-', "" },
        {',', "" },
         {'(', "" },
          {')', "" },
        {'a', "a" },
        {'b', "b" },
        {'c', "c" },
        {'d', "d" },
        {'e', "e" },
        {'f', "f" },
        {'g', "g" },
        {'h', "h" },
        {'i', "i" },
        {'j', "j" },
        {'k', "k" },
        {'l', "l" },
        {'m', "m" },
        {'n', "n" },
        {'o', "o" },
        {'p', "p" },
        {'r', "r" },
        {'s', "s" },
        {'t', "t" },
        {'q', "q" },
        {'u', "u" },
        {'v', "v" },
        {'w', "w" },
        {'x', "x" },
        {'y', "y" },
        {'z', "z" },
        {'A', "A" },
        {'B', "B" },
        {'C', "C" },
        {'D', "D" },
        {'E', "E" },
        {'F', "F" },
        {'G', "G" },
        {'H', "H" },
        {'I', "I" },
        {'J', "J" },
        {'K', "K" },
        {'L', "L" },
        {'M', "M" },
        {'N', "N" },
        {'O', "O" },
        {'P', "P" },
        {'R', "R" },
        {'S', "S" },
        {'T', "T" },
        {'Q', "Q" },
        {'U', "U" },
        {'V', "V" },
        {'W', "W" },
        {'X', "X" },
        {'Y', "Y" },
        {'Z', "Z" },
        {'1', "1" },
        {'2', "2" },
        {'3', "3" },
        {'4', "4" },
        {'5', "5" },
        {'6', "6" },
        {'7', "7" },
        {'8', "8" },
        {'9', "9" },
        {'0', "0" }
    };

    public static string ConvertToLatin(string source)
    {
        var result = new StringBuilder();
        foreach (var letter in source)
        {

            result.Append(ConvertedLetters[letter]);
        }
        return result.ToString();
    }
}