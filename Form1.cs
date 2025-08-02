using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Process currentProcess;
        private bool isDownloading = false;

        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0; // Selecciona la segunda opción 720P

            textBox2.BackColor = Color.Black;
            textBox2.ForeColor = Color.LightGreen;
        }

        private bool IsValidYoutubeUrl(string url)
        {
            // Ahora acepta /shorts/
            string pattern = @"^(https?:\/\/)?(www\.)?(youtube\.com\/(watch\?v=|shorts\/)|youtu\.be\/)[a-zA-Z0-9_-]+(\?.*)?$";
            return Regex.IsMatch(url, pattern);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (isDownloading)
            {
                // If already downloading, treat as Stop
                if (currentProcess != null && !currentProcess.HasExited)
                {
                    try
                    {
                        currentProcess.Kill();
                        currentProcess.Dispose();
                    }
                    catch { }
                }
                isDownloading = false;
                button1.Text = "Download";
                button1.Enabled = true;
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Value = 0;
                downloadInfoLabel.Text = "Download stopped by user.";
                MessageBox.Show("Download stopped by user.", "Stopped", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string url = textBoxUrl.Text.Trim();
            string outputPath = textBox3.Text.Trim();

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(outputPath))
            {
                MessageBox.Show("Please enter both URL and output path!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidYoutubeUrl(url))
            {
                MessageBox.Show("Please enter a valid YouTube URL!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(outputPath))
            {
                MessageBox.Show("Output directory does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedResolution = comboBox1.SelectedItem?.ToString();

            try
            {
                isDownloading = true;
                button1.Text = "Stop";
                button1.Enabled = true;
                progressBar1.Value = 0;
                textBox2.Clear();
                downloadInfoLabel.Text = "Initializing download...";

                int downloadStage = -1; // -1 = init, 0 = video, 1 = audio, 2 = merge, 3 = complete
                double videoProgress = 0;
                double audioProgress = 0;
        
                await Task.Run(() =>
                {
                    currentProcess = new Process();
                    string arguments;
                    if (selectedResolution == "AUTO")
                    {
                        arguments = $"-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]\" -o \"%(title)s_AUTO.%(ext)s\" --merge-output-format mp4 -P \"{outputPath}\" \"{url}\" --progress --no-playlist";
                    }
                    else
                    {
                        var match = Regex.Match(selectedResolution ?? "", @"(\d{3,4})");
                        if (!match.Success)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                MessageBox.Show("Please select a valid resolution!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            });
                            return;
                        }
                        int resolution = int.Parse(match.Groups[1].Value);
                        string resLabel = $"{resolution}P";
                        string formatArg = $"-f \"bestvideo[height={resolution}][ext=mp4]+bestaudio[ext=m4a]/best[height={resolution}][ext=mp4]\"";
                        arguments = $"{formatArg} -o \"%(title)s_{resLabel}.%(ext)s\" --merge-output-format mp4 -P \"{outputPath}\" \"{url}\" --progress --no-playlist";
                    }
                    currentProcess.StartInfo.FileName = "yt-dlp.exe";
                    currentProcess.StartInfo.Arguments = arguments;
                    currentProcess.StartInfo.UseShellExecute = false;
                    currentProcess.StartInfo.RedirectStandardOutput = true;
                    currentProcess.StartInfo.RedirectStandardError = true;
                    currentProcess.StartInfo.CreateNoWindow = true;

                    // Move progressBar1.Style update to UI thread before starting process
                    this.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Style = ProgressBarStyle.Marquee;
                        downloadInfoLabel.Text = "Initializing download...";
                    });

                    currentProcess.OutputDataReceived += (s, evt) =>
                    {
                        if (evt.Data != null)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                textBox2.AppendText(evt.Data + Environment.NewLine);

                                // Detectar inicio de descarga de video
                                if (evt.Data.Contains("Destination:") && downloadStage == -1)
                                {
                                    downloadStage = 0;
                                    progressBar1.Style = ProgressBarStyle.Continuous;
                                    progressBar1.Value = 0;
                                    downloadInfoLabel.Text = "Downloading video...";
                                }
                                // Detectar inicio de descarga de audio
                                else if (evt.Data.Contains("Destination:") && downloadStage == 0)
                                {
                                    downloadStage = 1;
                                    progressBar1.Value = 50;
                                    downloadInfoLabel.Text = "Downloading audio...";
                                }
                                // Detectar inicio de merge
                                else if (evt.Data.Contains("[Merger]"))
                                {
                                    downloadStage = 2;
                                    progressBar1.Style = ProgressBarStyle.Marquee;
                                    downloadInfoLabel.Text = "Merging audio and video...";
                                    return;
                                }
                                // Detectar finalización
                                else if (evt.Data.Contains("Deleting original file") && downloadStage == 2)
                                {
                                    downloadStage = 3;
                                    progressBar1.Style = ProgressBarStyle.Continuous;
                                    progressBar1.Value = 100;
                                    downloadInfoLabel.Text = "Download and merge completed";
                                    return;
                                }

                                var matchProgress = Regex.Match(evt.Data, @"(\d+\.?\d*)%\s+of.*?at\s+([\d\.]+[KMG]?i?B/s)\s+ETA\s+([\d:]+)");
                                if (matchProgress.Success)
                                {
                                    double percentage = double.Parse(matchProgress.Groups[1].Value);
                                    string speed = matchProgress.Groups[2].Value;
                                    string eta = matchProgress.Groups[3].Value;

                                    progressBar1.Style = ProgressBarStyle.Continuous;
                                    
                                    // Calcular el progreso según la etapa
                                    if (downloadStage == 0) // Video (0-50%)
                                    {
                                        int progressValue = (int)Math.Round(percentage * 0.5); // Multiplicar por 0.5 para llegar solo hasta 50%
                                        progressBar1.Value = Math.Max(0, Math.Min(50, progressValue));
                                        downloadInfoLabel.Text = $"Downloading video... {percentage:0}% - Speed: {speed} - ETA: {eta}";
                                    }
                                    else if (downloadStage == 1) // Audio (51-100%)
                                    {
                                        int progressValue = 50 + (int)Math.Round(percentage * 0.5); // Empezar desde 50% y llegar hasta 100%
                                        progressBar1.Value = Math.Max(50, Math.Min(100, progressValue));
                                        downloadInfoLabel.Text = $"Downloading audio... {percentage:0}% - Speed: {speed} - ETA: {eta}";
                                    }
                                }
                            });
                        }
                    };

                    currentProcess.ErrorDataReceived += (s, evt) =>
                    {
                        if (evt.Data != null)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                textBox2.AppendText("ERROR: " + evt.Data + Environment.NewLine);
                            });
                        }
                    };

                    currentProcess.Start();
                    currentProcess.BeginOutputReadLine();
                    currentProcess.BeginErrorReadLine();
                    currentProcess.WaitForExit();
                });

                MessageBox.Show("Download completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during download: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                downloadInfoLabel.Text = "Error during download.";
            }
            finally
            {
                isDownloading = false;
                button1.Text = "Download";
                button1.Enabled = true;
                currentProcess?.Dispose();
            }
        }

        private void buttonChoosePath_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the folder to save the video";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox3.Text = dialog.SelectedPath;
                }
            }
        }

        private void buttonChoosePath_Click_1(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox3.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Visible = checkBox1.Checked;
        }

        private void mainLayout_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
