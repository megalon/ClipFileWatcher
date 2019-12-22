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
using static System.Diagnostics.Debug;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Diagnostics;

namespace ClipFileWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FileSystemWatcher fileSystemWatcher = null;
        Process process;
        ProcessStartInfo processStartInfo;
        public MainWindow()
        {
            InitializeComponent();

            // instantiate the object
            fileSystemWatcher = new FileSystemWatcher();

            try
            {
                var pathToWatch = Properties.Settings.Default.PathToWatch;
                fileSystemWatcher.Path = pathToWatch;
                fileSystemWatcher.EnableRaisingEvents = true;
                UpdateFilePathLabel(pathToWatch);
                AppendToLog("Listening for changes in: " + fileSystemWatcher.Path);
            } catch {
                SelectFolder();
            }

            // Associate event handlers with the events
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
            fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
        }
        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            AppendToLog($"A file has been renamed from {e.OldName} to {e.Name}");
            if (e.Name.IndexOf(".clip") > 0)
            {
                AppendToLog($"Clip file was saved! Triggering script...");

                processStartInfo = new ProcessStartInfo();
                processStartInfo.UseShellExecute = false; // Hide popup cmd window
                processStartInfo.CreateNoWindow = true;   // Hide popup cmd window
                processStartInfo.WorkingDirectory = fileSystemWatcher.Path;
                processStartInfo.FileName = "git";
                processStartInfo.Arguments = "add .";
                process = new Process();
                Process.Start(processStartInfo).WaitForExit();

                processStartInfo.Arguments = "commit -a -m \"AUTO Saved File\"";
                Process.Start(processStartInfo);
            }
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            AppendToLog($"A new file has been deleted - {e.Name}");
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            AppendToLog($"A file has been changed - {e.Name}");
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            AppendToLog($"A new file has been created - {e.Name}");
        }

        private void Button_PathToWatch_Click(object sender, RoutedEventArgs e)
        {
            SelectFolder();
        }

        private void SelectFolder()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".clip";
            dlg.Filter = "CLIP Files (*.clip)|*.clip";
            dlg.Title = "Select the folder containing your .clip file";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                var folder = dlg.FileName.Substring(0, dlg.FileName.LastIndexOf('\\'));
                var filename = dlg.SafeFileName;
                // Do something with selected folder string
                AppendToLog("Watching file " + dlg.SafeFileName);
                AppendToLog("in folder " + folder);
                fileSystemWatcher.Path = folder;
                fileSystemWatcher.EnableRaisingEvents = true;
                Properties.Settings.Default.PathToWatch = folder;
                Properties.Settings.Default.Save();
                UpdateFilePathLabel(filename);

                if (!Directory.Exists(System.IO.Path.Combine(fileSystemWatcher.Path, ".git")))
                {
                    initFolder(dlg.SafeFileName);
                }
            }
            else
            {
                WriteLine("ERROR: Please select a valid folder!");
            }
        }

        private void initFolder(string filename)
        {
            processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = false; // Hide popup cmd window
            processStartInfo.CreateNoWindow = true;   // Hide popup cmd window
            processStartInfo.WorkingDirectory = fileSystemWatcher.Path;

            AppendToLog($"No .git folder found! Initializing git repo...");
            processStartInfo.FileName = "git";
            processStartInfo.Arguments = "init";
            process = new Process();
            Process.Start(processStartInfo).WaitForExit();

            AppendToLog($"Setting lfs to track .clip files...");
            processStartInfo.FileName = "git";
            processStartInfo.Arguments = "lfs track \"*.clip\"";
            process = new Process();
            Process.Start(processStartInfo).WaitForExit();

            AppendToLog($"Adding .gitattributes to repo...");
            processStartInfo.FileName = "git";
            processStartInfo.Arguments = "add .gitattributes";
            process = new Process();
            Process.Start(processStartInfo).WaitForExit();

            AppendToLog($"Initializing gitclip to track selected file...");
            processStartInfo.FileName = "gitclip";
            processStartInfo.Arguments = "init " + filename;
            process = new Process();
            Process.Start(processStartInfo).WaitForExit();

            AppendToLog($"Done! git repo and gitclip are setup to track {filename}!");
        }

        private void AppendToLog(string text)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TextBox_FileChangeEvents.Text += "\n" + text;
            });
        }

        private void UpdateFilePathLabel(string path)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Label_FilePath.Content = path;
            });
        }

        private void TextBox_FileChangeEvents_TextChanged(object sender, TextChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ScrollViewer_LogScroller.ScrollToBottom();
            });
        }
    }
}
