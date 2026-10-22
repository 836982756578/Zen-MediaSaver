// Zen Media Saver - Reklamsız Medya ve Müzik İndirici
// Geliştirici: Serkan ŞENTÜRK
// Telif Hakkı © 2026 Serkan ŞENTÜRK. Tüm Hakları Saklıdır.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ZenMediaSaver
{
    public class MainWindow : Window
    {
        private TextBox _urlBox;
        private ComboBox _formatCombo;
        private TextBox _folderBox;
        private Button _downloadBtn;
        private ProgressBar _progressBar;
        private TextBlock _progressStatus;
        private Border _progressCard;
        private StackPanel _historyList;
        private Process _currentProcess;

        public MainWindow()
        {
            Title = "Zen Media Saver";
            Width = 560;
            Height = 620;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;

            BuildUI();

            Loaded += (s, e) =>
            {
                CheckClipboardOnStartup();
            };
        }

        private void CheckClipboardOnStartup()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string txt = Clipboard.GetText().Trim();
                    if (txt.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || txt.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        if (txt.Contains("youtu") || txt.Contains("instagram") || txt.Contains("tiktok") || txt.Contains("twitter") || txt.Contains("x.com") || txt.Contains("facebook"))
                        {
                            _urlBox.Text = txt;
                        }
                    }
                }
            }
            catch { }
        }

        private void BuildUI()
        {
            Border rootBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(246, 22, 24, 34)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(20),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 26,
                    ShadowDepth = 6,
                    Opacity = 0.65
                }
            };

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Inputs
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Progress
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // History
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Footer

            // 1. Header (Icon + Title + Window buttons)
            Grid headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 16) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock logo = new TextBlock
            {
                Text = "☯️",
                FontSize = 22,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(logo, 0);

            StackPanel titleGroup = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            TextBlock title = new TextBlock
            {
                Text = "Zen Media Saver",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = Brushes.White
            };
            TextBlock subTitle = new TextBlock
            {
                Text = "Reklamsız 4K Video & MP3 Müzik İndirici • Serkan ŞENTÜRK",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(160, 165, 185)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            titleGroup.Children.Add(title);
            titleGroup.Children.Add(subTitle);
            Grid.SetColumn(titleGroup, 1);

            headerGrid.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed) DragMove();
            };

            StackPanel winBtns = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
            Button minBtn = CreateWinButton("—", () => WindowState = WindowState.Minimized);
            Button closeBtn = CreateWinButton("✕", () => Close());
            winBtns.Children.Add(minBtn);
            winBtns.Children.Add(closeBtn);
            Grid.SetColumn(winBtns, 2);

            headerGrid.Children.Add(logo);
            headerGrid.Children.Add(titleGroup);
            headerGrid.Children.Add(winBtns);
            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            // 2. Input Section
            StackPanel inputStack = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };

            // URL Label + Paste Button
            Grid urlHeader = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            urlHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            urlHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock urlLabel = new TextBlock
            {
                Text = "🔗 Medya Bağlantısı (URL):",
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 225, 240))
            };
            Grid.SetColumn(urlLabel, 0);

            Button pasteBtn = new Button
            {
                Content = "📋 Panodan Yapıştır",
                FontSize = 11,
                Padding = new Thickness(8, 2, 8, 2),
                Foreground = new SolidColorBrush(Color.FromRgb(100, 180, 255)),
                Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            pasteBtn.Click += (s, e) =>
            {
                try
                {
                    if (Clipboard.ContainsText()) _urlBox.Text = Clipboard.GetText().Trim();
                }
                catch { }
            };
            Grid.SetColumn(pasteBtn, 1);

            urlHeader.Children.Add(urlLabel);
            urlHeader.Children.Add(pasteBtn);
            inputStack.Children.Add(urlHeader);

            // URL Box
            Border urlBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(45, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(7),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 0, 0, 8)
            };
            _urlBox = new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.White,
                CaretBrush = Brushes.White,
                FontSize = 12.5
            };
            urlBorder.Child = _urlBox;
            inputStack.Children.Add(urlBorder);

            // Platform tags
            TextBlock supportedText = new TextBlock
            {
                Text = "✓ YouTube • Instagram Reels • TikTok • Twitter/X • Facebook • Pinterest • SoundCloud",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(140, 150, 175)),
                Margin = new Thickness(2, 0, 0, 12)
            };
            inputStack.Children.Add(supportedText);

            // Format + Destination Row
            Grid optionsGrid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            optionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.3, GridUnitType.Star) });
            optionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            optionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.7, GridUnitType.Star) });

            // Format Combo
            StackPanel formatStack = new StackPanel();
            formatStack.Children.Add(new TextBlock
            {
                Text = "Kalite / Format:",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 185, 200)),
                Margin = new Thickness(0, 0, 0, 4)
            });

            _formatCombo = new ComboBox { Height = 32, FontSize = 11.5 };
            _formatCombo.Items.Add("🎥 En Yüksek Video (4K / 1080p MP4)");
            _formatCombo.Items.Add("🎬 Standart Video (720p Hızlı MP4)");
            _formatCombo.Items.Add("🎵 MP3 Müzik (320 kbps)");
            _formatCombo.SelectedIndex = 0;
            formatStack.Children.Add(_formatCombo);
            Grid.SetColumn(formatStack, 0);

            // Output Folder
            StackPanel folderStack = new StackPanel();
            folderStack.Children.Add(new TextBlock
            {
                Text = "Kaydedilecek Klasör:",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 185, 200)),
                Margin = new Thickness(0, 0, 0, 4)
            });

            Grid folderGrid = new Grid();
            folderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            folderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string defaultDownloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            _folderBox = new TextBox
            {
                Text = Directory.Exists(defaultDownloads) ? defaultDownloads : Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Height = 32,
                FontSize = 11,
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                Padding = new Thickness(6, 6, 6, 6),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(_folderBox, 0);

            Button browseBtn = new Button
            {
                Content = "📁",
                Width = 32,
                Height = 32,
                FontSize = 13,
                Margin = new Thickness(4, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(45, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            browseBtn.Click += (s, e) =>
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _folderBox.Text = fbd.SelectedPath;
                    }
                }
            };
            Grid.SetColumn(browseBtn, 1);

            folderGrid.Children.Add(_folderBox);
            folderGrid.Children.Add(browseBtn);
            folderStack.Children.Add(folderGrid);
            Grid.SetColumn(folderStack, 2);

            optionsGrid.Children.Add(formatStack);
            optionsGrid.Children.Add(folderStack);
            inputStack.Children.Add(optionsGrid);

            // Download Button
            _downloadBtn = new Button
            {
                Content = "⬇️ Hemen İndir",
                Height = 40,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(225, 45, 75)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            _downloadBtn.Click += (s, e) => StartDownloadFlow();
            inputStack.Children.Add(_downloadBtn);

            Grid.SetRow(inputStack, 1);
            mainGrid.Children.Add(inputStack);

            // 3. Progress Card (Hidden initially)
            _progressCard = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 12),
                Visibility = Visibility.Collapsed
            };

            StackPanel progStack = new StackPanel();
            _progressStatus = new TextBlock
            {
                Text = "İndirme başlatılıyor...",
                FontSize = 11.5,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 6)
            };
            _progressBar = new ProgressBar
            {
                Height = 8,
                Minimum = 0,
                Maximum = 100,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 60, 90)),
                Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0))
            };
            progStack.Children.Add(_progressStatus);
            progStack.Children.Add(_progressBar);
            _progressCard.Child = progStack;

            Grid.SetRow(_progressCard, 2);
            mainGrid.Children.Add(_progressCard);

            // 4. Download History
            Border historyBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(25, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10)
            };

            ScrollViewer scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            _historyList = new StackPanel();

            TextBlock histTitle = new TextBlock
            {
                Text = "Son İndirilenler",
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 205, 220)),
                Margin = new Thickness(0, 0, 0, 8)
            };
            _historyList.Children.Add(histTitle);

            TextBlock emptyNotice = new TextBlock
            {
                Text = "Henüz indirilmiş medya yok.",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(140, 145, 160))
            };
            _historyList.Children.Add(emptyNotice);

            scroll.Content = _historyList;
            historyBorder.Child = scroll;

            Grid.SetRow(historyBorder, 3);
            mainGrid.Children.Add(historyBorder);

            // 5. Footer
            Grid footer = new Grid { Margin = new Thickness(0, 12, 0, 0) };
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock devText = new TextBlock
            {
                Text = "Geliştirici: Serkan ŞENTÜRK • © 2026",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(140, 145, 165)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(devText, 0);

            Button aboutBtn = new Button
            {
                Content = "ℹ️ Hakkında",
                FontSize = 11,
                Padding = new Thickness(10, 3, 10, 3),
                Foreground = new SolidColorBrush(Color.FromRgb(100, 180, 255)),
                Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            aboutBtn.Click += (s, e) =>
            {
                MessageBox.Show(
                    "Zen Media Saver - 4K Video & MP3 İndirici v1.0.0\n\n" +
                    "🌟 Geliştirici: Serkan ŞENTÜRK\n" +
                    "Telif Hakkı: © 2026 Serkan ŞENTÜRK. Tüm Hakları Saklıdır.\n\n" +
                    "YouTube, Instagram, TikTok, Twitter ve 1000'den fazla siteden reklamsız,\n" +
                    "güvenli ve en yüksek kalitede video/müzik indirmenizi sağlar.\n\n" +
                    "• 100% Bağımsız ve Taşınabilir (Zero-Install Portable)\n" +
                    "• 4K Ultra HD & 1080p MP4 ve 320 kbps MP3 Desteği\n" +
                    "• Sıfır Reklam, Sıfır Virüs\n\n" +
                    "GitHub: https://github.com/836982756578/Zen-MediaSaver",
                    "Zen Media Saver Hakkında",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            };
            Grid.SetColumn(aboutBtn, 1);

            footer.Children.Add(devText);
            footer.Children.Add(aboutBtn);
            Grid.SetRow(footer, 4);
            mainGrid.Children.Add(footer);

            rootBorder.Child = mainGrid;
            Content = rootBorder;
        }

        private void StartDownloadFlow()
        {
            string url = (_urlBox.Text ?? "").Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Lütfen indirmek istediğiniz video veya müziğin linkini girin.", "Link Gerekli", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string outDir = (_folderBox.Text ?? "").Trim();
            if (!Directory.Exists(outDir))
            {
                try { Directory.CreateDirectory(outDir); } catch { }
            }

            _downloadBtn.IsEnabled = false;
            _progressCard.Visibility = Visibility.Visible;
            _progressStatus.Text = "İndirme motoru hazırlanıyor...";
            _progressBar.Value = 0;
            _progressBar.IsIndeterminate = true;

            // Ensure yt-dlp is present
            DownloaderEngine.EnsureEngineAsync(
                onProgress: (pct) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _progressStatus.Text = string.Format("İndirme motoru güncelleniyor: %{0:F0}", pct);
                        _progressBar.IsIndeterminate = false;
                        _progressBar.Value = pct;
                    }));
                },
                onComplete: (success, errMsg) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!success)
                        {
                            _downloadBtn.IsEnabled = true;
                            _progressStatus.Text = errMsg;
                            MessageBox.Show(errMsg, "Motor Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Start actual download
                        _progressBar.IsIndeterminate = true;
                        _progressStatus.Text = "Video bilgileri alınıyor...";

                        DownloadFormat fmt = DownloadFormat.BestVideo;
                        if (_formatCombo.SelectedIndex == 1) fmt = DownloadFormat.Standard720;
                        else if (_formatCombo.SelectedIndex == 2) fmt = DownloadFormat.AudioMp3;

                        _currentProcess = DownloaderEngine.StartDownload(
                            url,
                            fmt,
                            outDir,
                            onProgress: (pArgs) =>
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _progressBar.IsIndeterminate = false;
                                    _progressBar.Value = pArgs.Percentage;
                                    _progressStatus.Text = pArgs.StatusText;
                                }));
                            },
                            onFinished: (dlSuccess, err, savedPath) =>
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _downloadBtn.IsEnabled = true;

                                    if (dlSuccess)
                                    {
                                        _progressBar.Value = 100;
                                        _progressStatus.Text = "🎉 İndirme başarıyla tamamlandı!";
                                        AddHistoryItem(savedPath ?? url);
                                        _urlBox.Text = "";
                                    }
                                    else
                                    {
                                        _progressStatus.Text = "Hata: " + err;
                                        MessageBox.Show(err, "İndirme Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                }));
                            }
                        );
                    }));
                }
            );
        }

        private void AddHistoryItem(string filePath)
        {
            if (_historyList.Children.Count == 2)
            {
                TextBlock tb = _historyList.Children[1] as TextBlock;
                if (tb != null && tb.Text.Contains("Henüz"))
                {
                    _historyList.Children.RemoveAt(1);
                }
            }

            Border card = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(0, 0, 0, 4)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string displayName = Path.GetFileName(filePath);
            if (string.IsNullOrEmpty(displayName)) displayName = filePath;

            TextBlock nameBlock = new TextBlock
            {
                Text = displayName,
                FontSize = 11,
                Foreground = Brushes.White,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameBlock, 0);

            Button openBtn = new Button
            {
                Content = "📁 Klasör",
                FontSize = 10,
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(4, 0, 0, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(100, 180, 255)),
                Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            openBtn.Click += (s, e) =>
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
                    }
                    else
                    {
                        Process.Start("explorer.exe", _folderBox.Text);
                    }
                }
                catch { }
            };
            Grid.SetColumn(openBtn, 1);

            Button playBtn = new Button
            {
                Content = "▶️ Oynat",
                FontSize = 10,
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(4, 0, 0, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(130, 225, 140)),
                Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            playBtn.Click += (s, e) =>
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                }
                catch { }
            };
            Grid.SetColumn(playBtn, 2);

            grid.Children.Add(nameBlock);
            grid.Children.Add(openBtn);
            grid.Children.Add(playBtn);
            card.Child = grid;

            _historyList.Children.Insert(1, card);
        }

        private Button CreateWinButton(string text, Action onClick)
        {
            Button btn = new Button
            {
                Content = text,
                Width = 28,
                Height = 26,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 185, 200)),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            btn.Click += (s, e) => onClick();
            return btn;
        }
    }
}
