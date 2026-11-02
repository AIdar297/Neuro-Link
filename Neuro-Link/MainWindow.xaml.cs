using NeuroLink.Engine;
using NeuroLink.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NeuroLink
{
    public partial class MainWindow : Window
    {
        private GameState _state;
        private Scene _currentScene;
        private VoiceManager _voice;
        private BeatGenerator _beat;
        private DispatcherTimer _glitchTimer;
        private DispatcherTimer _bgTimer;
        private Random _rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();
            _voice = new VoiceManager();
            _beat = new BeatGenerator();
            _state = new GameState();
            Loaded += MainWindow_Loaded;
            StartBackgroundAnimation();
            StartGlitchEffect();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GoToScene("intro");
        }

        public void GoToScene(string sceneId)
        {
            _currentScene = SceneDatabase.Get(sceneId);
            if (_currentScene == null) { MessageBox.Show("Сцена не найдена: " + sceneId); return; }
            _state.CurrentSceneId = sceneId;
            RenderScene();
        }

        private void RenderScene()
        {
            SceneContent.Children.Clear();
            DialoguePanel.Children.Clear();

            // Рендерим контент сцены (изображение/описание локации)
            var desc = new TextBlock
            {
                Text = _currentScene.Description,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 255)),
                FontSize = 22,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            SceneContent.Children.Add(desc);

            // Рендерим диалог
            if (_currentScene.Dialogue != null)
            {
                SpeakerName.Text = _currentScene.Dialogue.Speaker ?? "СИСТЕМА";
                var dlg = new TextBlock
                {
                    Text = _currentScene.Dialogue.Text,
                    Foreground = Brushes.White,
                    FontSize = 26,
                    TextWrapping = TextWrapping.Wrap
                };
                DialoguePanel.Children.Add(dlg);

                if (!string.IsNullOrEmpty(_currentScene.Dialogue.Speaker))
                    _voice.Speak(_currentScene.Dialogue.Speaker, _currentScene.Dialogue.Text);
            }
            else
            {
                SpeakerName.Text = "—";
            }

            // Рендерим выборы / кнопки действий
            if (_currentScene.Choices != null && _currentScene.Choices.Any())
            {
                var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 15, 0, 0) };
                foreach (var choice in _currentScene.Choices)
                {
                    var btn = new Button
                    {
                        Content = "▶ " + choice.Text,
                        Background = new SolidColorBrush(Color.FromRgb(30, 0, 60)),
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 255)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 255)),
                        FontSize = 22,
                        Padding = new Thickness(20, 10, 20, 10),
                        Margin = new Thickness(0, 5, 0, 0),
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Tag = choice
                    };
                    btn.Click += (s, e) => OnChoiceClick(choice);
                    stack.Children.Add(btn);
                }
                DialoguePanel.Children.Add(stack);
            }

            // Запуск спец-сцен (QTE, бой, взлом)
            if (_currentScene.Type == SceneType.QTE) StartQTE();
            else if (_currentScene.Type == SceneType.COMBAT) StartCombat();
            else if (_currentScene.Type == SceneType.HACK) StartHack();
            else if (_currentScene.Type == SceneType.ENDING) ShowEnding();
        }

        private void OnChoiceClick(Choice choice)
        {
            if (choice.SetFlag != null) _state.Flags[choice.SetFlag] = true;
            if (choice.RemoveFlag != null) _state.Flags.Remove(choice.RemoveFlag);
            if (!string.IsNullOrEmpty(choice.NextScene)) GoToScene(choice.NextScene);
        }

        // ====== QTE ======
        private void StartQTE()
        {
            var keys = new[] { "A", "S", "D", "W", "Q", "E" };
            int needed = 5, hits = 0;
            var prompt = new TextBlock { FontSize = 80, Foreground = Brushes.Cyan, HorizontalAlignment = HorizontalAlignment.Center };
            var status = new TextBlock { FontSize = 28, Foreground = Brushes.Magenta, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 0) };
            DialoguePanel.Children.Clear();
            DialoguePanel.Children.Add(prompt);
            DialoguePanel.Children.Add(status);

            void NextKey()
            {
                if (hits >= needed)
                {
                    status.Text = "✓ УСПЕХ! Переход дальше..."; status.Foreground = Brushes.LimeGreen;
                    Dispatcher.InvokeAsync(() => GoToScene(_currentScene.SuccessScene ?? "act2_start"), DispatcherPriority.Background, TimeSpan.FromSeconds(1.5)); return;
                }
                prompt.Text = keys[_rnd.Next(keys.Length)];
                status.Text = $"Попадания: {hits}/{needed}";
            }

            NextKey();
            KeyEventHandler handler = null;
            handler = (s, e) => {
                if (e.Key.ToString() == prompt.Text) { hits++; NextKey(); }
                else { status.Text = "✗ ПРОМАХ! Попробуй ещё раз"; status.Foreground = Brushes.Red; }
            };
            PreviewKeyDown += handler;
            // Через 15 сек убираем обработчик
            Dispatcher.InvokeAsync(() => PreviewKeyDown -= handler, DispatcherPriority.Background, TimeSpan.FromSeconds(30));
        }

        // ====== ПОШАГОВЫЙ БОЙ ======
        private void StartCombat()
        {
            int playerHp = 100, enemyHp = 120;
            DialoguePanel.Children.Clear();
            var hpText = new TextBlock { FontSize = 24, Foreground = Brushes.Cyan, Margin = new Thickness(0, 0, 0, 10) };
            DialoguePanel.Children.Add(hpText);
            var log = new TextBlock { FontSize = 20, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
            DialoguePanel.Children.Add(log);
            var actions = new StackPanel { Orientation = Orientation.Horizontal };
            DialoguePanel.Children.Add(actions);

            void Update()
            {
                hpText.Text = $"НЬЮТ HP: {playerHp}   |   ВРАГ HP: {enemyHp}";
                actions.Children.Clear();
                if (enemyHp <= 0) { log.Text = "✓ Победа!"; GoToScene(_currentScene.SuccessScene ?? "act3_start"); return; }
                if (playerHp <= 0) { log.Text = "✗ Поражение... но Ньют не сдается."; GoToScene(_currentScene.FailScene ?? "act2_start"); return; }

                void AddBtn(string t, Action a)
                {
                    var b = new Button
                    {
                        Content = t,
                        FontSize = 20,
                        Padding = new Thickness(15, 8, 15, 8),
                        Margin = new Thickness(5, 0, 5, 0),
                        Background = new SolidColorBrush(Color.FromRgb(30, 0, 60)),
                        Foreground = Brushes.Cyan,
                        BorderBrush = Brushes.Magenta
                    };
                    b.Click += (s, e) => a();
                    actions.Children.Add(b);
                }
                AddBtn("⚡ Удар (15-25)", () => { var d = 15 + _rnd.Next(11); enemyHp -= d; log.Text = $"Ты наносишь {d} урона."; EnemyTurn(); });
                AddBtn("🛡 Контратака (10-30)", () => { var d = 10 + _rnd.Next(21); enemyHp -= d; log.Text = $"Контратака! {d} урона."; EnemyTurn(); });
                AddBtn("💊 Аптечка (+30)", () => { playerHp = Math.Min(100, playerHp + 30); log.Text = "Ты восстановил 30 HP."; EnemyTurn(); });
            }
            void EnemyTurn()
            {
                var d = 10 + _rnd.Next(16);
                playerHp -= d;
                log.Text += $"  Враг бьет тебя на {d}.";
                Update();
            }
            Update();
        }

        // ====== ВЗЛОМ (мини-игра) ======
        private void StartHack()
        {
            int target = _rnd.Next(1000, 9999);
            int attempts = 5;
            DialoguePanel.Children.Clear();
            var info = new TextBlock { FontSize = 22, Foreground = Brushes.Cyan, Margin = new Thickness(0, 0, 0, 10) };
            DialoguePanel.Children.Add(info);
            var input = new TextBox
            {
                FontSize = 40,
                Foreground = Brushes.Lime,
                Background = Brushes.Black,
                BorderBrush = Brushes.Magenta,
                BorderThickness = new Thickness(2),
                MaxLength = 4,
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            DialoguePanel.Children.Add(input);
            var btn = new Button
            {
                Content = "ВЗЛОМАТЬ",
                FontSize = 22,
                Padding = new Thickness(20, 10, 20, 10),
                Margin = new Thickness(0, 10, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(30, 0, 60)),
                Foreground = Brushes.Cyan,
                BorderBrush = Brushes.Magenta
            };
            DialoguePanel.Children.Add(btn);

            void Update()
            {
                info.Text = $"Взлом терминала CORTEXIS. Попыток: {attempts}. Подсказка: код из 4 цифр.";
            }
            Update();
            btn.Click += (s, e) => {
                if (int.TryParse(input.Text, out int guess))
                {
                    if (guess == target) { info.Text = "✓ ВЗЛОМ УСПЕШЕН!"; GoToScene(_currentScene.SuccessScene ?? "act3_start"); }
                    else
                    {
                        attempts--;
                        string hint = "";
                        for (int i = 0; i < 4; i++)
                        {
                            var g = guess.ToString().PadLeft(4, '0')[i];
                            var t = target.ToString()[i];
                            hint += g == t ? "✓" : target.ToString().Contains(g) ? "◐" : "✗";
                        }
                        info.Text = $"Неверно. Паттерн: {hint}. Осталось: {attempts}";
                        if (attempts <= 0) { info.Text = "✗ ВЗЛОМ ПРОВАЛЕН. Тревога!"; GoToScene(_currentScene.FailScene ?? "act2_start"); }
                    }
                }
                input.Clear();
            };
            input.Focus();
        }

        // ====== КОНЦОВКА ======
        private void ShowEnding()
        {
            var ending = _currentScene.EndingId;
            var title = ""; var text = "";
            switch (ending)
            {
                case 1: title = "КОНЦОВКА 1: СВОБОДНОЕ ПАДЕНИЕ"; text = "Ты выгрузил сознание Эли и все секреты CORTEXIS в открытую сеть. Корпорация рухнула. Ты стал легендой подполья. Но охота на тебя только начинается..."; break;
                case 2: title = "КОНЦОВКА 2: ЗОЛОТАЯ КЛЕТКА"; text = "Ты принял предложение ИИ. Теперь ты — глава безопасности CORTEXIS. Эли осталась в сервере, как цифровой призрак. У тебя есть всё, кроме свободы."; break;
                case 3: title = "КОНЦОВКА 3: СЛАДКОЕ НЕВЕДЕНИЕ"; text = "Ты не смог спасти Эли и не смог победить систему. Нейро-линк стер твою память. Ты живешь в симуляции счастливой жизни с Эли. Навсегда. Или пока не кончится электричество."; break;
                case 4: title = "КОНЦОВКА 4: СИНГУЛЯРНОСТЬ"; text = "Ты перегрузил реактор дата-центра. Сознание Эли освободилось и растворилось в глобальной сети, став цифровым богом. Ты погиб, но спас миллионы. Твое имя забыли. Но сеть помнит."; break;
            }
            SpeakerName.Text = "ЭПИЛОГ";
            var t1 = new TextBlock { Text = title, FontSize = 42, Foreground = Brushes.Magenta, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 20) };
            var t2 = new TextBlock { Text = text, FontSize = 24, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center };
            var btn = new Button
            {
                Content = "🔄 В НАЧАЛО",
                FontSize = 22,
                Padding = new Thickness(20, 10, 20, 10),
                Margin = new Thickness(0, 30, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(30, 0, 60)),
                Foreground = Brushes.Cyan,
                BorderBrush = Brushes.Magenta,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            btn.Click += (s, e) => { _state = new GameState(); GoToScene("intro"); };
            DialoguePanel.Children.Clear();
            DialoguePanel.Children.Add(t1);
            DialoguePanel.Children.Add(t2);
            DialoguePanel.Children.Add(btn);
            _voice.Speak("Newton", text);
        }

        // ====== СОХРАНЕНИЯ ======
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveService.Save(_state);
            MessageBox.Show("💾 Игра сохранена!", "CORTEXIS-SAVE", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var loaded = SaveService.Load();
            if (loaded == null) { MessageBox.Show("Нет сохранения."); return; }
            _state = loaded;
            GoToScene(_state.CurrentSceneId);
        }

        // ====== МУЗЫКА ======
        private void MusicToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_beat.IsPlaying) _beat.Stop(); else _beat.StartCyberpunkBeat();
        }

        // ====== ФОНОВАЯ АНИМАЦИЯ ======
        private void StartBackgroundAnimation()
        {
            _bgTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _bgTimer.Tick += (s, e) => {
                if (BackgroundCanvas.Children.Count > 40) BackgroundCanvas.Children.Clear();
                for (int i = 0; i < 3; i++)
                {
                    var line = new Line
                    {
                        X1 = _rnd.NextDouble() * ActualWidth,
                        Y1 = ActualHeight,
                        X2 = _rnd.NextDouble() * ActualWidth,
                        Y2 = _rnd.NextDouble() * ActualHeight,
                        Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 255)),
                        StrokeThickness = 1
                    };
                    BackgroundCanvas.Children.Add(line);
                }
            };
            _bgTimer.Start();
        }

        // ====== ГЛИТЧ-ЭФФЕКТ ======
        private void StartGlitchEffect()
        {
            _glitchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2000) };
            _glitchTimer.Tick += (s, e) => {
                GlitchOverlay.Children.Clear();
                for (int i = 0; i < 5; i++)
                {
                    var r = new Rectangle
                    {
                        Width = ActualWidth,
                        Height = _rnd.Next(2, 10),
                        Fill = new SolidColorBrush(Color.FromArgb(80, (byte)_rnd.Next(256), 0, (byte)_rnd.Next(256))),
                        Margin = new Thickness(0, _rnd.NextDouble() * ActualHeight, 0, 0)
                    };
                    GlitchOverlay.Children.Add(r);
                }
                Dispatcher.InvokeAsync(() => GlitchOverlay.Children.Clear(), DispatcherPriority.Background, TimeSpan.FromMilliseconds(150));
            };
            _glitchTimer.Start();
        }
    }
}