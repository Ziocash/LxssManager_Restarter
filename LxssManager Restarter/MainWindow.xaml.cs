using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
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
using System.Windows.Threading;

namespace LxssManager_Restarter
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum Mode
        {
            Restart,
            Start,
            Stop
        }
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            timer.Start();
            CheckServices();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CheckServices();
        }

        private void CheckServices()
        {
            ObservableCollection<string> itemSource = new ObservableCollection<string>();
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();

            foreach (ServiceController scTemp in scServices)
            {
                if (scTemp.Status == ServiceControllerStatus.Running && scTemp.DisplayName.Equals("LxssManager"))
                {
                    try
                    {
                        ManagementObject wmiService;
                        wmiService = new ManagementObject("Win32_Service.Name='" + scTemp.ServiceName + "'");
                        wmiService.Get();
                        itemSource.Add(scTemp.DisplayName);
                    }
                    catch (Exception) { }
                }
            }
            if (itemSource.Count > 0)
            {
                servicesCheckBox.ItemsSource = itemSource;
                servicesCheckBox.SelectedIndex = 0;
                servicesCheckBox.Visibility = Visibility.Visible;
                outputText.Text = "LxssManager in esecuzione";
                restartButton.IsEnabled = true;
                stopButton.IsEnabled = true;
                startButton.IsEnabled = false;
                labelService.Visibility = Visibility.Visible;
            }
            else
            {
                servicesCheckBox.Visibility = Visibility.Hidden;
                outputText.Text = "LxssManager non avviato";
                restartButton.IsEnabled = false;
                stopButton.IsEnabled = false;
                startButton.IsEnabled = true;
                labelService.Visibility = Visibility.Hidden;
            }
        }

        private bool RunCommand(string command, Mode mode)
        {
            try
            {
                if (mode == Mode.Restart)
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "net.exe";
                    p.StartInfo.Arguments = $"stop {command}";
                    p.Start();
                    outputText.Text = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "net.exe";
                    p.StartInfo.Arguments = $"start {command}";
                    p.Start();
                    string appo = outputText.Text;
                    appo += Environment.NewLine;
                    appo += p.StandardOutput.ReadToEnd();
                    outputText.Text = appo;
                    p.WaitForExit();
                    return true;
                }
                else if(mode == Mode.Stop)
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "net.exe";
                    p.StartInfo.Arguments = $"stop {command}";
                    p.Start();
                    outputText.Text = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return true;
                }
                else
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "net.exe";
                    p.StartInfo.Arguments = $"start {command}";
                    p.Start();
                    outputText.Text = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return true;
                }
            }
            catch(Exception)
            {
                return false;
            }
            
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            outputText.Text = null;
            if(RunCommand(servicesCheckBox.Items[servicesCheckBox.SelectedIndex].ToString(), Mode.Restart))
                MessageBox.Show("Riavvio completato", "Riavvio", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Impossibile proseguire", "Riavvio fallito", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            outputText.Text = null;
            if (RunCommand("LxssManager", Mode.Start))
                MessageBox.Show("Avvio completato", "Avvio", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Impossibile proseguire", "Avvio fallito", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            outputText.Text = null;
            if (RunCommand(servicesCheckBox.Items[servicesCheckBox.SelectedIndex].ToString(), Mode.Stop))
                MessageBox.Show("Arresto completato", "Arresto", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Impossibile proseguire", "Arresto fallito", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
