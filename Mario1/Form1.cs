using Mario1.GameObject.Gameobject;
using Mario1.GameObject.Gameobject.Creature;
using System.Diagnostics;

namespace Mario1
{
    public partial class Form1 : Form
    {
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;
        private Button btnStart;

        private Stopwatch _sw = Stopwatch.StartNew();
        private double _deltaTime;
        private double _timeScale;


        private Image[] _digitImages = new Image[10];
        private int spawn;//отвечает когда спавнить что либо
        public int lives;
        private List<Creature> creatures;
        private List<Block> blocks;
        private List<Background> backgrounds;
        private Mario mario;
        private Navigator nav;
        internal void StopGameTimer() => gameTimer.Stop();
        internal void StartGameTimer() => gameTimer.Start();

        // Гглавный игровой таймер
        private System.Windows.Forms.Timer gameTimer;

        public Form1()
        {
            InitializeComponent();
            // Настройка формы для производительной отрисовки (Double buffering)
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            KeyPreview = true; // Позволяет форме перехватывать события клавиатуры
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 10;
            gameTimer.Tick += GameTimer_Tick;
            if (screenWidth == 0) screenWidth = 2000;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Menu();
        }

        private void Menu()
        {
            BackColor = Color.Black;
            btnStart = new Button();
            btnStart.Size = new Size(280, 107);
            btnStart.BackgroundImage = Properties.Resources.start;
            btnStart.Location = new Point((screenWidth - btnStart.Size.Width) / 2, screenHeight - screenHeight / 3);
            btnStart.CausesValidation = false;
            Controls.Add(btnStart);
            btnStart.Click += button1_Click;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GameStart();
            btnStart.Dispose();
        }

        private void GameStart()
        {
            _digitImages[0] = Properties.Resources.num_0;
            _digitImages[1] = Properties.Resources.num_1;
            _digitImages[2] = Properties.Resources.num_2;
            _digitImages[3] = Properties.Resources.num_3;
            _digitImages[4] = Properties.Resources.num_4;
            _digitImages[5] = Properties.Resources.num_5;
            _digitImages[6] = Properties.Resources.num_6;
            _digitImages[7] = Properties.Resources.num_7;
            _digitImages[8] = Properties.Resources.num_8;
            _digitImages[9] = Properties.Resources.num_9;
            spawn = 0;
            lives = 3;
            creatures = new List<Creature>();
            blocks = new List<Block>();
            backgrounds = new List<Background>();
            nav = new Navigator();
            mario = new Mario(100, 700, nav.base_height_ordinary("ordinary"), nav.base_width_ordinary);
            Spawn_Load();
            gameTimer.Start();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            AnimationManager.Clean();

            // Таймер
            if (gameTimer != null)
            {
                gameTimer?.Stop();
                gameTimer?.Dispose();
            }
        }

        // Главный игровой цикл
        public void GameTimer_Tick(object sender, EventArgs e)
        {
            Stabilization_Time();
            if (!AnimationManager.IsAnimating)
            {
                Left_Mario();
                Right_Mario(false);
                Sliding_Mario();
                Jump_Mario();
                Padenie_Mario();
            }
            Actions();
            for (short i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].Animation(metod: "", triger: 70) & creatures[i].State.HasFlag(CreatureState.Intangible) & creatures[i].name == "Image_Goomba")
                    { creatures.RemoveAt(i--); if (i == -1) break; }
                i = Jump_сreatures(i);
                if (i == -1) break;
                i = Fall_сreatures(i);
                if (i == -1) break;
                i = Creatures_come_out(i);
                if (i == -1) break;
                i = Movement_creatures_X(i);
                if (i == -1) break;
                if (creatures[i].X < -200)
                {
                    creatures.RemoveAt(i--);
                    if (i == -1) break;
                }
                
            }
            Сhecking_blocks();
            mario.Intangible_Mario();
            if (mario.pause_atack_fire_bar > 0) mario.pause_atack_fire_bar -= 1;
            this.Invalidate();
            if (mario.Y + mario.height >= 3000) Dead_mario_restart();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (mario is null) return;
            for (int i = 0; i < backgrounds.Count; i++)
            {
                if (backgrounds[i].X > -500 & backgrounds[i].X < screenWidth)
                e.Graphics.DrawImage(backgrounds[i].image, backgrounds[i].X, backgrounds[i].Y);
            }
            for (int i = creatures.Count - 1; i >= 0; i--)
            {
                if (creatures[i].X > 0 - creatures[i].width & creatures[i].X < screenWidth)
                {
                    if (creatures[i].height <= creatures[i].proper_height) e.Graphics.DrawImage(creatures[i].image, creatures[i].X, creatures[i].Y, creatures[i].DestRect(), GraphicsUnit.Pixel);
                    else e.Graphics.DrawImage(creatures[i].image, creatures[i].X, creatures[i].Y);
                }
            }
            if (mario.image is not null)
            {
                if (!mario.deadPadeniye)
                {
                    if (mario.Y > 0 - mario.height & mario.Y < screenHeight)
                    {
                        e.Graphics.DrawImage(mario.image, mario.X - (((83 - nav.base_width_ordinary) / 2)), mario.Y);
                    }
                }
            }
            for (int i = 0; i < blocks.Count; i++)
            {
                if ((blocks[i].X > 0 - blocks[i].width & blocks[i].X < screenWidth))
                {
                    e.Graphics.DrawImage(blocks[i].image, blocks[i].X, blocks[i].Y);
                }
            }
            if (mario.image is not null)
            {
                if (mario.deadPadeniye)
                {
                    if (mario.Y > 0 - mario.height & mario.Y < screenHeight)
                    {
                        e.Graphics.DrawImage(mario.image, mario.X - (((83 - nav.base_width_ordinary) / 2)), mario.Y);
                    }
                }
            }
            DrawCoinCounter(e);
        }

        private void DrawCoinCounter(PaintEventArgs e)
        {
            if (mario == null || _digitImages[0] == null) return;

            int coins = mario.coin;
            // D3 -> 000, 005, 099 | D2 -> 00, 05, 99 (формат)
            string coinStr = coins.ToString("D2");

            // Настройки позиции и размера
            int startX = screenWidth - 270; 
            int startY = 15;                
            int digitWidth = 70;            
            int digitHeight = 70;           
            int spacing = 10;                     // Расстояние между цифрами
            
            int currentX = startX;
            if (coins == 0) 
            { }
            e.Graphics.DrawImage(Properties.Resources.Coin, currentX, startY, digitWidth, digitHeight);
            currentX += digitWidth + spacing + 7;
            foreach (char c in coinStr)
            {
                int digit = c - '0'; // Преобразуем символ '0'..'9' в число 0..9
                if (_digitImages[digit] != null)
                {
                    e.Graphics.DrawImage(_digitImages[digit], currentX, startY, digitWidth, digitHeight);
                    currentX += digitWidth + spacing;
                }
            }
        }

        private void Stabilization_Time()
        {
            _deltaTime = _sw.Elapsed.TotalMilliseconds; //Замеряем, сколько реального времени прошло с прошлого кадра
            _sw.Restart();                              //Мгновенно обнуляем таймер и запускаем заново
            _timeScale = Math.Clamp(_deltaTime / 16.67, 0.5, 2.0);
        }

        private void Left_Mario()
        {
            if (mario.TimerLeft)
            {
                if (mario.X > 0)
                {
                    int moveStep = (int)(mario.speed * _timeScale);
                    for (short y = 0; y < moveStep; y++)
                    {
                        mario.X--;
                        for (short i = 0; i < creatures.Count; i++)
                        {
                            if (mario.mode != "intangible ordinary" & !mario.deadPadeniye & creatures[i].property != "Attack against creatures" & !creatures[i].State.HasFlag(CreatureState.DeadFall) & !creatures[i].State.HasFlag(CreatureState.Intangible))
                            {
                                if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }) == true)
                                {
                                    i = Mario_in_сreatures(i);
                                }
                            }
                        }
                        for (short i = 0; i < blocks.Count; i++)
                        {
                            if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { blocks[i].X, blocks[i].X + blocks[i].width, blocks[i].Y, blocks[i].Y + blocks[i].height }))
                            {
                                mario.TimerSliding = false;
                                mario.TimerLeft = false;
                                mario.X = blocks[i].X + blocks[i].width;
                                mario.run_animation = 0;
                                if (!mario.sits) mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
                                mario.speed = 1;
                                return;
                            }
                        }
                        if (Paday_Mario(false)) break;
                    }
                    if (mario.run_animation % 5 == 0 & mario.speed < mario.max_speed & !mario.TimerSliding)
                    {
                        if (!mario.acceleration) mario.speed++;
                        else mario.speed += 2;
                    }
                    else if (mario.run_animation % 5 == 0 & mario.speed > mario.max_speed & !mario.TimerSliding) mario.speed--;
                    if (mario.we_stand != -1)
                        if (mario.Y + mario.height == blocks[mario.we_stand].Y & !mario.braking2 & !mario.sits)
                            mario.Defines_the_image("Walk");
                    mario.run_animation += 1;
                }
                else
                {
                    mario.run_animation = 0;
                    mario.TimerSliding = false;
                    mario.TimerLeft = false;
                    if (!mario.sits) mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
                    mario.speed = 1;
                }
            }
        }

        private void Right_Mario(bool intangible_block)
        {
            if (mario.TimerRight)
            {
                if (mario.X < screenWidth / 2 - screenWidth / 10 || spawn + screenWidth > nav.end_location_X)
                {
                    if (mario.X + mario.width < screenWidth)
                    {
                        int moveStep = (int)(mario.speed * _timeScale);
                        for (short y = 0; y < moveStep; y++)
                        {
                            mario.X++;
                            if (!intangible_block)
                            {
                                for (short i = 0; i < creatures.Count; i++)
                                {
                                    if (mario.mode != "intangible ordinary" & !mario.deadPadeniye & creatures[i].property != "Attack against creatures" & !creatures[i].State.HasFlag(CreatureState.DeadFall) & !creatures[i].State.HasFlag(CreatureState.Intangible))
                                    {
                                        if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }) == true)
                                        {
                                            i = Mario_in_сreatures(i);
                                        }
                                    }
                                }
                                for (short i = 0; i < blocks.Count; i++)
                                {
                                    if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { blocks[i].X, blocks[i].X + blocks[i].width, blocks[i].Y, blocks[i].Y + blocks[i].height }) == true)
                                    {
                                        if (blocks[i].property == "between_locations" & blocks[i].name == "Column")
                                        {
                                            mario.we_stand = i - 1; //то есть блок на котором стоит Column
                                            mario.TimerLeft = false;
                                            mario.TimerRight = false;
                                            mario.TimerGravity = false;
                                            mario.TimerSliding = false;
                                            mario.TimerSpace = false;
                                            mario.g = 1;
                                            mario.spaceG_max = 15;
                                            mario.spaceG = 20;
                                            mario.stopForm1_KeyDown = true;
                                            mario.run_animation = 0;
                                            mario.speed = 1;
                                            AnimationManager.PlayAnimation(
                                                durationMs: 2000,
                                                intervalMs: 5,
                                                onFrame: frame =>
                                                {
                                                    Anim_finish("Column", i);
                                                },
                                                onComplete: () =>
                                                {
                                                    Switching_between_locations(i);
                                                }
                                            );
                                            return;
                                        }
                                        else if ((blocks[i].property == "between_locations" || blocks[i].property == "return_between_locations") & blocks[i].name == "90Pipe_input" & mario.Y > blocks[i].Y)
                                        {
                                            mario.TimerLeft = false;
                                            mario.TimerRight = true;
                                            mario.TimerGravity = false;
                                            mario.TimerSliding = false;
                                            mario.TimerSpace = false;
                                            mario.g = 1;
                                            mario.spaceG_max = 15;
                                            mario.spaceG = 20;
                                            mario.stopForm1_KeyDown = true;
                                            AnimationManager.PlayAnimation(
                                                durationMs: 500,      // 0.8 секунды
                                                intervalMs: 5,
                                                onFrame: frame =>
                                                {
                                                    // Логика анимации (каждый тик таймера)
                                                    Anim_finish("90Pipe_input", i);
                                                },
                                                onComplete: () =>
                                                {
                                                    if (blocks[i].property == "between_locations")
                                                        Switching_between_locations(i);
                                                    else
                                                        Switching_return_between_locations(return_location: blocks[i].location_transfer);
                                                }
                                            );
                                            return;
                                        }
                                        mario.TimerSliding = false;
                                        mario.TimerRight = false;
                                        mario.X = blocks[i].X - mario.width;
                                        mario.run_animation = 0;
                                        if (!mario.sits) mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
                                        mario.speed = 1;
                                        return;
                                    }
                                }
                            }
                            if (Paday_Mario(true)) break;
                        }
                        if (mario.run_animation % 5 == 0 & mario.speed < mario.max_speed & !mario.TimerSliding)
                        {
                            if (!mario.acceleration) mario.speed++;
                            else mario.speed += 2;
                        }
                        else if (mario.run_animation % 5 == 0 & mario.speed > mario.max_speed & !mario.TimerSliding) mario.speed--;
                        if (mario.we_stand != -1)
                            if (mario.Y + mario.height == blocks[mario.we_stand].Y & !mario.braking2 & !mario.sits)
                                mario.Defines_the_image("Walk");
                    }
                    else
                    {
                        mario.run_animation = 0;
                        mario.TimerSliding = false;
                        mario.TimerRight = false;
                        if (!mario.sits) mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
                        mario.speed = 1;
                    }
                }
                else
                {
                    int moveStep = (int)(mario.speed * _timeScale);
                    for (short y = 0; y < moveStep; y++)
                    {
                        for (short i = 0; i < blocks.Count; i++)
                        {
                            if (Сheck(new int[] { mario.X, mario.X + mario.width + 1, mario.Y, mario.Y + mario.height }, new int[] { blocks[i].X, blocks[i].X + blocks[i].width, blocks[i].Y, blocks[i].Y + blocks[i].height }) == true)
                            {
                                if (blocks[i].property == "return_between_locations")
                                {
                                    Switching_return_between_locations(return_location: blocks[i].location_transfer);
                                    return;
                                }
                                else if (blocks[i].property == "between_locations" & blocks[i].name != "Pipe")
                                {
                                    Switching_between_locations(i);
                                    return;
                                }
                                mario.TimerSliding = false;
                                mario.TimerRight = false;
                                mario.X = blocks[i].X - mario.width;
                                mario.run_animation = 0;
                                if (!mario.sits) mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
                                mario.speed = 1;
                                return;
                            }
                        }
                        for (short i = 0; i < blocks.Count; i++)
                        {
                            blocks[i].X--;
                            if (blocks[i].X < -200)
                            {
                                blocks.RemoveAt(i);
                                for (int r = 0; r < mario.nam.Count; r++)
                                {
                                    if (mario.nam[r] == i) mario.nam.RemoveAt(r--);
                                    else if (mario.nam[r] > i) mario.nam[r]--;
                                }
                                creatures = blocks[i].Check_сreature_we_stand(creatures, i);
                                if (mario.we_stand > i) mario.we_stand--;
                                i--;
                            }
                        }
                        for (short i = 0; i < creatures.Count; i++)
                        {
                            if (mario.mode != "intangible ordinary" & creatures[i].property != "Attack against creatures" & !mario.deadPadeniye & !creatures[i].State.HasFlag(CreatureState.DeadFall) & !creatures[i].State.HasFlag(CreatureState.Intangible))
                            {
                                if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }) == true)
                                {
                                    i = Mario_in_сreatures(i);
                                }
                            }
                        }
                        for (short i = 0; i < creatures.Count; i++) creatures[i].X--;
                        for (short i = 0; i < backgrounds.Count; i++) backgrounds[i].X--;
                        spawn++;
                        Spawn_Load();
                        if (Paday_Mario(true)) break;
                    }
                    if (mario.run_animation % 5 == 0 & mario.speed < mario.max_speed & !mario.TimerSliding)
                    {
                        if (!mario.acceleration) mario.speed++;
                        else mario.speed += 2;
                    }
                    else if (mario.run_animation % 5 == 0 & mario.speed > mario.max_speed & !mario.TimerSliding) mario.speed--;
                    if (mario.we_stand != -1)
                        if (mario.Y + mario.height == blocks[mario.we_stand].Y & !mario.braking2 & !mario.sits)
                            mario.Defines_the_image("Walk");
                }
                mario.run_animation += 1;
            }
        }
        
        private void Sliding_Mario()
        {
            if (mario.TimerSliding)
            {
                if (mario.run_animation % 10 == 0)
                {
                    if (mario.speed <= 1)
                    {
                        mario.speed = 1;
                        if (!mario.sits) mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
                        mario.run_animation = 0;
                        mario.TimerLeft = false;
                        mario.TimerRight = false;
                        mario.TimerSliding = false;
                        mario.braking2 = false;
                    }
                    else
                    {
                        if (mario.braking2)
                        {
                            mario.speed -= 2;
                            if (mario.braking)
                            {
                                if (mario.direction) mario.direction = false;
                                else mario.direction = true;
                                if (!mario.sits) mario.Defines_the_image("Skid");
                                mario.braking = false;
                            }
                        }
                        if (mario.speed > 1) mario.speed -= 2;
                    }
                }
            }
        }

        private void Jump_Mario()
        {
            if (mario.TimerSpace)
            {
                if (mario.spaceG > 0)
                {
                    if (!mario.deadPadeniye)
                    {
                        mario.we_stand = -1;
                        mario.Y -= (int)(mario.spaceG * _timeScale);
                        if (mario.spaceG_max != 0) mario.spaceG_max--;
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { blocks[i].X, blocks[i].X + blocks[i].width, blocks[i].Y, blocks[i].Y + blocks[i].height }))
                            {
                                bool flag = false;
                                int sravnenieTverdoeCosanieL = blocks[i].X + blocks[i].width - mario.X;
                                for (int m = i + 1; m < blocks.Count; m++)
                                {
                                    if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { blocks[m].X, blocks[m].X + blocks[m].width, blocks[m].Y, blocks[m].Y + blocks[m].height }))
                                    {
                                        int sravnenieTverdoeCosanieR = mario.X + mario.width - blocks[m].X;
                                        if (sravnenieTverdoeCosanieR > sravnenieTverdoeCosanieL)
                                        {
                                            i = m;
                                        }
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    int width = 30;
                                    bool flag2 = false;
                                    if (!mario.TimerRight & mario.X + mario.width > blocks[i].X & mario.X + mario.width < blocks[i].X + width) 
                                        { flag2 = true; flag = true; }
                                    else if (!mario.TimerLeft & mario.X < blocks[i].X + blocks[i].width & mario.X > blocks[i].X + blocks[i].width - width) 
                                        { flag2 = false; flag = true; }
                                    bool flag3 = false;
                                    for (int m = 0; m < blocks.Count; m++)
                                    {
                                        if (!flag) { flag3 = true; break; }
                                        if (m == i) continue;
                                        if (blocks[m].Y == blocks[i].Y & 
                                            (
                                                (flag2 & blocks[m].X < blocks[i].X & blocks[m].X + blocks[m].width > blocks[i].X - mario.width) 
                                                ||
                                                (!flag2 & blocks[m].X > blocks[i].X & blocks[m].X < blocks[i].X + blocks[i].width + mario.width)
                                            )
                                        )
                                            { flag3 = true; break; }
                                    }
                                    if (!flag3)
                                    {
                                        if (flag2) mario.X = blocks[i].X - mario.width;
                                        else mario.X = blocks[i].X + blocks[i].width;
                                        return;
                                    }
                                }
                                int t = proverka_sovp_dvuh_perem_spiskov(i);
                                if (t != -1)
                                {
                                    blocks[t].sluchay[0] = 14 - blocks[t].sluchay[0];
                                }
                                else
                                {
                                    mario.nam.Add(i);
                                    blocks[i].sluchay = [0, blocks[i].Y];
                                }
                                mario.Y = blocks[mario.nam[mario.nam.Count - 1]].Y + blocks[mario.nam[mario.nam.Count - 1]].height;
                                mario.spaceG = 20;
                                mario.spaceG_max = 15;
                                mario.TimerSpace = false;
                                mario.TimerGravity = true;
                                break;
                            }
                        }
                    }
                    if (!mario.spaceG_bool || mario.spaceG_max == 0) mario.spaceG--;
                }
                else
                {
                    mario.spaceG_max = 15;
                    mario.spaceG = 20;
                    mario.TimerSpace = false;
                    mario.TimerGravity = true;
                }
            }
        }

        private void Padenie_Mario()
        {
            if (mario.TimerGravity)
            {
                if (mario.Y + mario.height < mario.top)
                {
                    mario.Y += (int)(mario.g * _timeScale);
                    mario.g++;
                    if (!mario.deadPadeniye)
                    {
                        for (short i = 0; i < blocks.Count; i++)
                        {
                            if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { blocks[i].X, blocks[i].X + blocks[i].width, blocks[i].Y, blocks[i].Y + blocks[i].height }))
                            {
                                mario.Y = (blocks[i].Y - (mario.Y + mario.height - mario.Y));
                                mario.g = 1;
                                mario.we_stand = i;
                                mario.TimerGravity = false;
                                if (mario.sits) mario.Defines_the_image("Duck");
                                else mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
                            }
                        }
                        if (mario.mode != "intangible ordinary")
                        {
                            for (short i = 0; i < creatures.Count; i++)
                            {
                                if (creatures[i].State.HasFlag(CreatureState.DeadFall) || creatures[i].State.HasFlag(CreatureState.Intangible)) continue;
                                if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }))
                                {
                                    i = Creatures_in_mario(i);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (mario.sits) mario.Defines_the_image("Duck");
                    else mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
                    mario.g = 1;
                    mario.TimerGravity = false;
                }
            }
        }

        private bool Paday_Mario(bool direction)
        {
            if (mario.we_stand != -1 & !mario.TimerSpace)
            {
                if (mario.X + mario.width < blocks[mario.we_stand].X || mario.X > (blocks[mario.we_stand].X + blocks[mario.we_stand].width))
                {
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (mario.we_stand == i) continue;
                        if(blocks[i].Y == blocks[mario.we_stand].Y & ((!direction & blocks[i].X + blocks[i].width > blocks[mario.we_stand].X - 15 & blocks[i].X < blocks[mario.we_stand].X) || (direction & blocks[i].X < blocks[mario.we_stand].X + blocks[mario.we_stand].width + 15 & blocks[i].X > blocks[mario.we_stand].X))) 
                        { mario.we_stand = i; return false; }
                    }
                    mario.we_stand = -1;
                    mario.TimerGravity = true;
                    return true;
                }
            }
            return false;
        }

        private void Actions()
        {
            for (int i = 0; i < mario.nam.Count; i++)
            {
                if (blocks[mario.nam[i]].sluchay is null) continue;
                int r = mario.nam[i];
                switch (blocks[r].name)
                {
                    case "Bricks":
                        if (mario.mode == "ordinary" || mario.mode == "intangible ordinary")
                        {
                            if (blocks[r].sluchay[0] >= 0 & blocks[r].sluchay[0] < 7)
                            {
                                blocks[r].Y -= 2;
                                Knocking_out_enemy_creatures_with_a_block(r);
                                blocks[r].sluchay[0] += 1;
                            }
                            else if (blocks[r].sluchay[0] >= 7 & blocks[r].sluchay[0] < 14)
                            {
                                blocks[r].Y += 2;
                                blocks[r].sluchay[0] += 1;
                            }
                            else
                            {
                                if (blocks[r].Y < blocks[r].sluchay[1])
                                {
                                    blocks[r].Y += 1;
                                }
                                else if (blocks[r].Y > blocks[r].sluchay[1])
                                {
                                    blocks[r].Y -= 1;
                                }
                                else
                                {
                                    blocks[r].sluchay = null;
                                    mario.nam.RemoveAt(i);
                                    i -= 1;
                                }
                            }
                        }
                        else if (mario.mode == "big ordinary" || mario.mode == "big shooter")
                        {
                            blocks[r].Y -= 2;
                            Knocking_out_enemy_creatures_with_a_block(r);
                            blocks.RemoveAt(r);
                            mario.nam.RemoveAt(i);
                            for (int t = 0; t < mario.nam.Count; t++)
                            {
                                if (mario.nam[t] == i) mario.nam.RemoveAt(t--);
                                else if (mario.nam[t] > i) mario.nam[t]--;
                            }
                            creatures = blocks[i].Check_сreature_we_stand(creatures, i);
                            if (mario.we_stand > i) mario.we_stand--;
                            i--;
                        }
                        break;
                        
                    case "LuckyBlock":
                        if (blocks[r].sluchay[0] == 0)
                        {
                            blocks[r].name = "Iron";
                            blocks[r].image = Properties.Resources.EmptyBlock;
                            blocks[r].Y -= 2;
                            Knocking_out_enemy_creatures_with_a_block(r);
                            blocks[r].sluchay[0] += 1;
                        }
                        break;

                    case "Pipe":
                        if (blocks[r].sluchay[1] >= mario.Y)
                        {
                            mario.Y += 1;
                            blocks[r].sluchay[0] += 1;
                            mario.TimerRight = false;
                            mario.TimerLeft = false;
                            mario.TimerGravity = false;
                            mario.TimerSpace = false;
                            mario.TimerSliding = false;
                        }
                        else
                        {
                            Switching_between_locations(r);
                            return;
                        }
                        break;
                        
                    case "Iron":
                        if (blocks[r].sluchay[0] >= 1 & blocks[r].sluchay[0] < 7)
                        {
                            blocks[r].Y -= 2;
                            Knocking_out_enemy_creatures_with_a_block(r);
                            if (blocks[r].sluchay[0] == 6)
                            {
                                if (blocks[r].property == "mushroom/flower bonus")
                                {
                                    if (mario.mode == "ordinary" || mario.mode == "intangible ordinary") creatures.Add(new Creature(x: blocks[r].X, y: blocks[r].sluchay[1], name: "mushroom bonus", height: 0, width: 83, direction: true, property: "bonus", top: 3000, g: 1, spaceG: 0, condition: new List<string> { "stands" }, image: Properties.Resources.Super_Mushroom, proper_height: 83));
                                    else if (mario.mode == "big ordinary" || mario.mode == "big shooter") creatures.Add(new Creature ( x: blocks[r].X, y: blocks[r].sluchay[1], name: "flower bonus", height: 0, width: 83, direction: true, property: "bonus", top: 3000, g: 1, spaceG: 0, condition: new List<string> { "stands" }, image: Properties.Resources.Fire_Flower__1_, proper_height: 83));
                                }
                                //исключение в top!!! (сюда сохраняется значение sluchay[r][1], чтобы сравнить с реальным spawnY_creatures для удаления при достижении определённой высоты)
                                else if (blocks[mario.nam[i]].property == "money") creatures.Add(new Creature(x: blocks[r].X + (blocks[r].width / 2) - 16, y: blocks[r].sluchay[1], name: "money bonus", height: 0, width: 32, direction: true, property: "bonus", top: blocks[r].sluchay[1], g: 1, spaceG: 0, condition: new List<string> { "stands", "coin_up" }, image: Properties.Resources.Coin, proper_height: 56));
                            }
                            blocks[r].sluchay[0] += 1;
                        }
                        else if (blocks[r].sluchay[0] >= 7 & blocks[r].sluchay[0] < 14)
                        {
                            blocks[r].Y += 2;
                            blocks[r].sluchay[0] += 1;
                        }
                        else
                        {
                            if (blocks[r].Y < blocks[r].sluchay[1])
                            {
                                blocks[r].Y += 1;
                            }
                            else if (blocks[r].Y > blocks[r].sluchay[1])
                            {
                                blocks[r].Y -= 1;
                            }
                            else
                            {
                                blocks[r].Y = blocks[r].sluchay[1];
                                blocks[r].sluchay = null;
                                mario.nam.RemoveAt(i);
                                i -= 1;
                            }
                        }
                        break;
                }
            }
        }

        private short Creatures_come_out(short i)
        {
            if (creatures[i].property == "bonus" & creatures[i].State.HasFlag(CreatureState.Stands))
            {
                if (creatures[i].name == "mushroom bonus" || creatures[i].name == "flower bonus")
                {
                    if (creatures[i].height >= creatures[i].proper_height)
                    {
                        if (creatures[i].name == "mushroom bonus") creatures[i].State &= ~CreatureState.Stands;
                    }
                    else
                    {
                        creatures[i].Y -= 1;
                        creatures[i].height += 1;
                    }
                }
                else if (creatures[i].name == "money bonus")
                {
                    if (creatures[i].State.HasFlag(CreatureState.CoinUp))
                    {
                        if (creatures[i].height < creatures[i].proper_height)
                        {
                            if (!creatures[i].State.HasFlag(CreatureState.CoinFalse))
                            {
                                mario.coin += 1;
                                creatures[i].State |= CreatureState.Intangible | CreatureState.CoinFalse;
                                if (mario.coin >= mario.coin_max)
                                {
                                    mario.coin = 0;
                                    lives++;
                                }
                            }
                            creatures[i].Y -= 6;
                            creatures[i].height += 6;
                        }
                        else
                        {
                            creatures[i].Y -= 6;
                            if (creatures[i].Y < creatures[i].top - 150)// top_creatures[r] из исключения!!! (вводится при создании money bonus)
                            {
                                creatures.RemoveAt(i--);
                            }
                        }
                    }
                }
            }
            return i;
        }

        private short Movement_creatures_X(short i)
        {
            if (creatures[i].State.HasFlag(CreatureState.DeadFall)) return i;
            if (!creatures[i].State.HasFlag(CreatureState.Stands))
            {
                if (creatures[i].direction)
                {
                    creatures[i].X += (int)(creatures[i].speed * _timeScale);
                    for (short r = 0; r < blocks.Count; r++)
                    {
                        if (creatures[i].State.HasFlag(CreatureState.Intangible)) break;
                        if (Сheck(new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }, new int[] { blocks[r].X, blocks[r].X + blocks[r].width, blocks[r].Y, blocks[r].Y + blocks[r].height }) == true)
                        {
                            creatures[i].X = blocks[r].X - creatures[i].width;
                            creatures[i].direction = false;
                        };
                    }
                    Check_block_we_stand(i);
                }
                else
                {
                    creatures[i].X -= (int)(creatures[i].speed * _timeScale);
                    for (short r = 0; r < blocks.Count; r++)
                    {
                        if (creatures[i].State.HasFlag(CreatureState.Intangible)) break;
                        if (Сheck(new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }, new int[] { blocks[r].X, blocks[r].X + blocks[r].width, blocks[r].Y, blocks[r].Y + blocks[r].height }) == true)
                        {
                            creatures[i].X = blocks[r].X + blocks[r].width;
                            creatures[i].direction = true;
                        }
                    }
                    Check_block_we_stand(i);
                }
                if (!creatures[i].State.HasFlag(CreatureState.AttackOnEveryone)) creatures[i].Animation("Walk");
            }
            for (short r = 0; r < creatures.Count; r++)
            {
                if (creatures[i].State.HasFlag(CreatureState.Intangible)) break;
                if (r == i || creatures[r].State.HasFlag(CreatureState.Stands) || creatures[r].State.HasFlag(CreatureState.Intangible)) continue;
                if (Сheck(new int[] { creatures[r].X, creatures[r].X + creatures[r].width, creatures[r].Y, creatures[r].Y + creatures[r].height }, new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }) == true)
                {
                    if ((creatures[r].property == "Attack against creatures" & creatures[i].property == "") || (creatures[i].property == "Attack against creatures" & creatures[r].property == ""))
                    {
                        creatures.RemoveAt(i);
                        if (i < r) { r -= 1; i -= 1; }
                        else i -= 2;
                        creatures.RemoveAt(r);
                        if (i == -1) return i;
                        if (r == -1) break;
                    }
                    else if (creatures[i].State.HasFlag(CreatureState.AttackOnEveryone) & !creatures[r].State.HasFlag(CreatureState.AttackOnEveryone))
                    {
                         creatures[r].Dead_fall();
                    }
                    else if (creatures[r].State.HasFlag(CreatureState.AttackOnEveryone) & !creatures[i].State.HasFlag(CreatureState.AttackOnEveryone))
                    {
                        creatures[i].Dead_fall();
                    }
                    else if ((creatures[r].property != "Attack against creatures" & creatures[i].property != "Attack against creatures") || (creatures[i].State.HasFlag(CreatureState.AttackOnEveryone) & creatures[r].State.HasFlag(CreatureState.AttackOnEveryone)))
                    {
                        if (creatures[i].direction != true) creatures[i].direction = true;
                        else creatures[i].direction = false;
                        if (creatures[r].direction != false) creatures[r].direction = false;
                        else creatures[r].direction = true;
                    }
                }
            }
            if (creatures[i].State.HasFlag(CreatureState.WaitingForMario)) 
            {
                if (!Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }) == true)
                {
                    creatures[i].State &= ~CreatureState.WaitingForMario & ~CreatureState.DoesntKill;
                }
            }
            else if (mario.mode != "intangible ordinary" & !mario.deadPadeniye & !creatures[i].State.HasFlag(CreatureState.DoesntKill) & !creatures[i].State.HasFlag(CreatureState.Intangible)) 
            {
                if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }) == true)
                    return Mario_in_сreatures(i);
            }
            return i;
        }
        
        private short Jump_сreatures(short i)
        {
            if (creatures[i].spaceG > 0 & !creatures[i].TimerGravity)
            {
                creatures[i].Y -= (int)(creatures[i].spaceG * _timeScale);
                creatures[i].spaceG--;
                if (creatures[i].we_stand != -1) creatures[i].we_stand = -1;
                if (!creatures[i].State.HasFlag(CreatureState.Stands) & !creatures[i].State.HasFlag(CreatureState.Intangible))
                {
                    for (short r = 0; r < blocks.Count; r++)
                    {
                        if (Сheck(new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }, new int[] { blocks[r].X, blocks[r].X + blocks[r].width, blocks[r].Y, blocks[r].Y + blocks[r].height }) == true)
                        {
                            creatures[i].Y = blocks[r].Y + blocks[r].height;
                            creatures[i].spaceG = 0;
                        }
                    }
                }
                if (mario.mode != "intangible ordinary" || !creatures[i].State.HasFlag(CreatureState.Intangible))
                {
                    if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }) == true)
                    {
                        return Creatures_in_mario(i);
                    }
                }
            }
            else
            {
                if (creatures[i].State.HasFlag(CreatureState.Jump))
                {
                    creatures[i].spaceG = creatures[i].spaceG_const;
                    creatures[i].TimerGravity = true;
                }
            }
            return i;
        }

        private short Fall_сreatures(short i)
        {
            if (creatures[i].property == "bonus" & creatures[i].State.HasFlag(CreatureState.Stands)) return i;
            if (creatures[i].TimerGravity)
            {
                creatures[i].Y += (int)(creatures[i].g * _timeScale);
                creatures[i].g++;
                if (creatures[i].State.HasFlag(CreatureState.DeadFall) || creatures[i].State.HasFlag(CreatureState.Intangible)) return i;
                for (short r = 0; r < blocks.Count; r++)
                {
                    if (Сheck(new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }, new int[] { blocks[r].X, blocks[r].X + blocks[r].width, blocks[r].Y, blocks[r].Y + blocks[r].height }) == true)
                    {
                        creatures[i].Y = (blocks[r].Y - creatures[i].height);
                        creatures[i].we_stand = r;
                        creatures[i].TimerGravity = false;
                    }
                }
                if (mario.mode != "intangible ordinary" & !mario.deadPadeniye & !creatures[i].State.HasFlag(CreatureState.DoesntKill))
                {
                    if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }) == true)
                        return Mario_in_сreatures(i);
                }
            }
            else creatures[i].g = 1;
            return i;
        }

        private void Сhecking_blocks()
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].property == "up->teleport_down->up")
                {
                    if (blocks[i].Y + blocks[i].height + 100 <= 0) blocks[i].Y = screenHeight + 100;
                    blocks[i].Y -= 5;
                    if (Сheck(new int[] { mario.X, mario.X + mario.width, mario.Y, mario.Y + mario.height }, new int[] { blocks[i].X, blocks[i].X + blocks[i].width, blocks[i].Y, blocks[i].Y + blocks[i].height }))
                    {
                        mario.Y = blocks[i].Y - mario.height;
                    }
                    for (int r = 0; r < creatures.Count; r++)
                    {
                        if (Сheck(new int[] { creatures[r].X, creatures[r].X + creatures[r].width, creatures[r].Y, creatures[r].Y + creatures[r].height }, new int[] { blocks[i].X, blocks[i].X + blocks[i].width, blocks[i].Y, blocks[i].Y + blocks[i].height }))
                        {
                            creatures[r].Y = blocks[i].Y - creatures[r].height;
                        }
                    }
                }
            }
        }

        private short Creatures_in_mario(short i)
        {
            if (creatures[i].name == "mushroom bonus" || creatures[i].name == "flower bonus")
            {
                if (mario.mode == "ordinary")
                {
                    mario.Y -= nav.base_height_ordinary(mario.mode); ;
                    mario.mode = "big ordinary";
                    mario.width = nav.base_width_ordinary;
                    mario.height = nav.base_height_ordinary(mario.mode);

                    mario.Defines_the_image("Super Mario");
                }
                else if (mario.mode == "big ordinary")
                {
                    mario.mode = "big shooter";
                    mario.Defines_the_image("Fiery Mario");
                }
                creatures.RemoveAt(i--);
                return i;
            }
            else if (creatures[i].name == "money bonus")
            {
                mario.coin++;
                if (mario.coin >= mario.coin_max) { mario.coin = 0; lives++; }
                creatures.RemoveAt(i--);
                return i;
            }
            else if (creatures[i].property == "")
            {
                mario.Y -= mario.g;
                mario.g = 1;
                mario.spaceG = 15;
                mario.TimerGravity = false;
                mario.TimerSpace = true;
                
                if (creatures[i].name == "SMB_greenkoopatroopa")
                {
                    if (creatures[i].State.HasFlag(CreatureState.DoesntKill))
                    {
                        creatures[i].State &= ~CreatureState.Stands;
                        creatures[i].State |= CreatureState.WaitingForMario | CreatureState.AttackOnEveryone;
                        creatures[i].speed = 15;
                        if (mario.X + (mario.width/2) <= creatures[i].X + (creatures[i].width / 2)) creatures[i].direction = true;
                        else creatures[i].direction = false;
                    }
                    else
                    {
                        creatures[i].g = 1;
                        creatures[i].spaceG = 0;
                        creatures[i].height = 38;
                        creatures[i].width = 45;
                        creatures[i].TimerGravity = true;
                        creatures[i].State |= CreatureState.DoesntKill | CreatureState.Stands;
                        creatures[i].State &= ~CreatureState.Jump;
                        creatures[i].Animation("Dead");
                    }
                        
                }
                else if (creatures[i].name == "SMB_greenparatrooper")
                {
                    creatures[i].name = "SMB_greenkoopatroopa";
                    creatures[i].State &= ~CreatureState.Jump;
                    creatures[i].TimerGravity = true;
                    creatures[i].spaceG = 0;
                    creatures[i].image = Properties.Resources.SMB_greenkoopatroopa1;
                }
                else if (creatures[i].name == "Image_Goomba")
                {
                    creatures[i].g = 15;
                    creatures[i].height = 40;
                    if (creatures[i].we_stand == -1) creatures[i].TimerGravity = true;
                    else creatures[i].Y = blocks[creatures[i].we_stand].Y - creatures[i].height;
                    creatures[i].run_animation = 0;
                    creatures[i].State |= CreatureState.Intangible | CreatureState.Stands | CreatureState.DoesntKill;
                    creatures[i].Animation("Dead");
                }
            }
            return i;
        }

        private short Mario_in_сreatures(short i)
        {
            if (creatures[i].State.HasFlag(CreatureState.DoesntKill) & (creatures[i].name == "SMB_greenparatrooper" || creatures[i].name == "SMB_greenkoopatroopa"))
            {
                creatures[i].State &= ~CreatureState.Stands;
                creatures[i].State |= CreatureState.WaitingForMario | CreatureState.AttackOnEveryone;
                creatures[i].speed = 15;
                if (mario.X + (mario.width / 2) <= creatures[i].X + (creatures[i].width / 2)) creatures[i].direction = true;
                else creatures[i].direction = false;
            }
            else if (creatures[i].name == "mushroom bonus" || creatures[i].name == "flower bonus")
            {
                if (mario.mode == "ordinary")
                {
                    mario.Y -= nav.base_height_ordinary(mario.mode);
                    mario.mode = "big ordinary";
                    mario.width = nav.base_width_ordinary;
                    mario.height = nav.base_height_ordinary(mario.mode);
                    mario.Defines_the_image("Super Mario");
                }
                else if (mario.mode == "big ordinary")
                {
                    mario.mode = "big shooter";
                    mario.Defines_the_image("Fiery Mario");
                }
                creatures.RemoveAt(i--);
            }
            else if (creatures[i].name == "money bonus")
            {
                mario.coin++;
                if (mario.coin >= mario.coin_max) { mario.coin = 0; lives++; }
                creatures.RemoveAt(i--);
                return i;
            }
            else if (creatures[i].property == "")
            {
                if (mario.mode == "ordinary")
                {
                    mario.Mario_Dead();
                }
                else if (mario.mode == "big ordinary" || mario.mode == "big shooter")
                {
                    mario.mode = "intangible ordinary";
                    mario.width = nav.base_width_ordinary;
                    mario.height = nav.base_height_ordinary(mario.mode);
                    mario.Y += nav.base_height_ordinary(mario.mode);
                    Get_up_from_your_squats();
                    mario.Defines_the_image("Mario");
                }
            }
            return i;
        }

        private void Check_block_we_stand(int creatures_i)
        {
            if (creatures[creatures_i].we_stand == -1 || creatures[creatures_i].TimerGravity) return;
            int blocks_i = creatures[creatures_i].we_stand;
            if
            (
                (
                    creatures[creatures_i].X + creatures[creatures_i].width) < blocks[blocks_i].X
                    ||
                    creatures[creatures_i].X > (blocks[blocks_i].X + blocks[blocks_i].width
                )
            )
            {
                creatures[creatures_i].TimerGravity = true;
                creatures[creatures_i].top = 3000;
                creatures[creatures_i].we_stand = -1;
            }
        }

        private void Get_up_from_your_squats()
        {
            if (mario.sits == true)
            {
                if (mario.mode == "intangible ordinary")
                {
                    mario.Y -= nav.base_height_ordinary("big ordinary") - nav.base_height_ordinary("sits");
                }
                else if (mario.mode == "big ordinary")
                {
                    mario.Defines_the_image("Super Mario");
                    mario.width = nav.base_width_ordinary;
                    mario.height = nav.base_height_ordinary(mario.mode);
                    mario.Y -= nav.base_height_ordinary(mario.mode) - nav.base_height_ordinary("sits");
                }
                else if (mario.mode == "big shooter")
                {
                    mario.Defines_the_image("Fiery Mario");
                    mario.width = nav.base_width_ordinary;
                    mario.height = nav.base_height_ordinary(mario.mode);
                    mario.Y -= nav.base_height_ordinary(mario.mode) - nav.base_height_ordinary("sits");
                }
                mario.sits = false;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (mario is null) return;
            if (e.KeyCode == Keys.Left & !mario.TimerSliding & mario.TimerLeft)
            {
                mario.TimerSliding = true;
            }
            if (e.KeyCode == Keys.Right & !mario.TimerSliding & mario.TimerRight)
            {
                mario.TimerSliding = true;
            }
            if (e.KeyCode == Keys.Down)
            {
                Get_up_from_your_squats();
            }
            if (e.KeyCode == Keys.Up)
            {
                mario.spaceG_bool = false;
            }
            if (e.KeyCode == Keys.ShiftKey)
            {
                mario.max_speed = 10;
                mario.acceleration = false;
            }
        }
        
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (mario is null) return;
            if (!mario.stopForm1_KeyDown)
            {
                if (e.KeyCode == Keys.Left & !mario.sits)
                {
                    if (mario.TimerRight)
                    {
                        mario.TimerSliding = true;
                        mario.braking = true;
                        mario.braking2 = true;
                    }
                    else if ((!mario.TimerLeft & !mario.TimerSliding & !mario.TimerRight) || (mario.TimerLeft & mario.TimerSliding & !mario.TimerRight))
                    {
                        mario.braking2 = false;
                        mario.braking = false;
                        mario.TimerLeft = true;
                        mario.direction = false;
                        mario.TimerSliding = false;
                        if (mario.mode == "big shooter" & mario.pause_atack_fire_bar == 0)
                            Get_up_from_your_squats();
                    }
                }

                if (e.KeyCode == Keys.Right & !mario.sits)
                {
                    if (mario.TimerLeft)
                    {
                        mario.TimerSliding = true;
                        mario.braking = true;
                        mario.braking2 = true;
                    }
                    else if ((!mario.TimerRight & !mario.TimerSliding & !mario.TimerLeft) || (mario.TimerRight & mario.TimerSliding & !mario.TimerLeft))
                    {
                        mario.braking2 = false;
                        mario.braking = false;
                        mario.TimerRight = true;
                        mario.direction = true;
                        mario.TimerSliding = false;
                        if (mario.mode == "big shooter" & mario.pause_atack_fire_bar == 0)
                            Get_up_from_your_squats();
                    }
                }

                if (e.KeyCode == Keys.Space)
                {
                    if (mario.mode == "big shooter" & mario.pause_atack_fire_bar == 0)
                    {
                        creatures.Add(new Creature(x: mario.X + 32, y: mario.Y + 32, direction: mario.direction, name: "Fire bar", width: 16, height: 16, condition: new List<string> { "jump" }, property: "Attack against creatures", g: 1, spaceG: 17, top: 3000, image: Properties.Resources.Fire_bar));
                        mario.pause_atack_fire_bar = 50;
                    }
                }

                if (e.KeyCode == Keys.ShiftKey)
                {
                    mario.max_speed = 14;
                    mario.acceleration = true;
                }

                if(e.KeyCode == Keys.Up)
                {
                    if (!mario.TimerGravity & mario.we_stand != -1 & !mario.TimerSpace)
                    {
                        if (mario.sits) { mario.sits = false; mario.height = nav.base_height_ordinary(""); mario.Y -= nav.base_height_ordinary("") - nav.base_height_ordinary("sits"); }
                        mario.spaceG_bool = true;
                        mario.Defines_the_image("Jump");
                        mario.TimerSpace = true;
                    }
                }

                if(e.KeyCode == Keys.Down)
                {
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (mario.Y + mario.height == blocks[i].Y & blocks[i].name == "Pipe" & blocks[i].property == "between_locations")
                        {
                            if (mario.X > blocks[i].X & mario.X + mario.width < blocks[i].X + blocks[i].width)
                            {
                                mario.stopForm1_KeyDown = true;
                                mario.nam.Add(i);
                                blocks[i].sluchay = [0, blocks[i].Y];
                                break;
                            }
                        }
                        else if (i == blocks.Count - 1 & (mario.mode == "big ordinary" || mario.mode == "big shooter") & mario.sits == false)
                        {
                            mario.TimerSliding = true;
                            mario.sits = true;
                            mario.width = nav.base_width_ordinary;
                            mario.height = nav.base_height_ordinary("sits");
                            mario.Y += nav.base_height_ordinary(mario.mode) - nav.base_height_ordinary("sits");
                            mario.Defines_the_image("Duck");
                        }
                    }
                }
            }
        }

        private void Knocking_out_enemy_creatures_with_a_block(int nam)
        {
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].State.HasFlag(CreatureState.Stands)) continue;  
                if (Сheck(new int[] { creatures[i].X, creatures[i].X + creatures[i].width, creatures[i].Y, creatures[i].Y + creatures[i].height }, new int[] { blocks[nam].X, blocks[nam].X + blocks[nam].width, blocks[nam].Y, blocks[nam].Y + blocks[nam].height }) == true)
                {
                    creatures[i].Dead_fall();
                }
            }
        }

        private bool Сheck(int[] object_one, int[] object_two)
        {
            if (object_one[1] > object_two[0] & object_one[2] < object_two[3] & object_one[3] > object_two[2] & object_one[0] < object_two[1])
            {
                return true;
            }
            return false;
        }

        private int proverka_sovp_dvuh_perem_spiskov(int i)
        {
            foreach (int t in mario.nam)
            {
                if (t == i) return t;
            }
            return -1;
        }

        public void Anim_finish(string variant, int i_block)
        {
            if (variant == "Column")
            {
                //TimerSpace в данном методе исспользуется не по назначению !!! (чтобы упорядочить действия и не создавать доп. переменной)
                //run_animation в данном методе исспользуется не по назначению!!!(чтобы упорядочить действия и не создавать доп.переменной)
                if (mario.Y + mario.height < 762) { mario.Y += 3; mario.TimerSpace = true; }
                else if (mario.TimerSpace)
                {
                    mario.X += 93;
                    mario.TimerSpace = false;
                    mario.direction = false;
                    mario.Defines_the_image("Jump");
                    mario.direction = true;
                    mario.run_animation = 30;
                }
                else if (mario.run_animation > 15 & !mario.TimerRight) mario.run_animation--;
                else if (mario.run_animation > 0 & !mario.TimerRight) { mario.run_animation--; mario.Defines_the_image("Mario/Super Mario/Fiery Mario"); }
                else
                {
                    if (!mario.TimerRight) { mario.TimerRight = true; mario.speed = 2; }
                    Right_Mario(true);
                    Padenie_Mario();
                    for (int i = 0; i < backgrounds.Count; i++)
                    {
                        if (backgrounds[i].name == "Finish" & mario.X >= backgrounds[i].X + 100) AnimationManager._currentFrame = AnimationManager._maxFrames;
                    }
                }
            }
            else if (variant == "90Pipe_input")
            {
                if (mario.X < blocks[i_block].X + blocks[i_block].height)
                {
                    if (mario.Y > blocks[i_block].Y + blocks[i_block].height - mario.height - 18)
                        mario.Y--;
                    Right_Mario(true);
                }
            }
        }

        private void Switching_return_between_locations(int return_location)
        {
            for (int i = 0; i < nav.navigator[return_location].Count; i++)
            {
                object[] location_object = nav.navigator[return_location][i];
                if ((string)location_object[0] == "Block")
                {
                    if ((string)location_object[7] == "exit_from_the_location") 
                    {
                        if ((int)location_object[8] == nav.current_location)
                        {
                            for (int r = 0; r < i - 5; r++)
                            {
                                if ((string)nav.navigator[return_location][r][0] != "Settings")
                                {
                                    nav.navigator[return_location].RemoveAt(r--);
                                    i--;
                                }
                            }
                            creatures = new List<Creature>();
                            blocks = new List<Block>();
                            backgrounds = new List<Background>();
                            mario.nam = new List<int>();
                            spawn = (int)location_object[1] - 200;
                            mario.we_stand = -1;
                            mario.X = (int)location_object[1] - spawn + (int)location_object[5] / 2 - mario.width / 2;
                            mario.Y = (int)location_object[2];
                            mario.direction = true;
                            nav.Switching_between_locations(return_location);
                            mario.top = 3000;
                            mario.TimerRight = false;
                            mario.TimerGravity = false;
                            mario.TimerSliding = false;
                            mario.TimerSpace = false;
                            mario.g = 1;
                            mario.spaceG_max = 15;
                            mario.spaceG = 20;
                            mario.stopForm1_KeyDown = true;
                            mario.run_animation = 0;
                            mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
                            mario.speed = 1;
                            Spawn_Load();
                            AnimationManager.PlayAnimation(
                                durationMs: 800,      // 0.8 секунды
                                intervalMs: 5,
                                onFrame: frame =>
                                {
                                    // Логика анимации (каждый тик таймера)
                                    if (frame < mario.height / 2 + mario.height % 2)
                                    {
                                        mario.Y -= 2; // Подбрасываем вверх
                                    }
                                    else 
                                    {
                                        AnimationManager._currentFrame = AnimationManager._maxFrames;
                                    }
                                },
                                onComplete: () =>
                                {
                                    mario.stopForm1_KeyDown = false; // Разблокируем ввод
                                    mario.TimerGravity = true;
                                }
                            );
                        }
                    }
                }
            }
        }

        private void Switching_between_locations(int t)
        {
            nav.Switching_between_locations(blocks[t].location_transfer);
            int current_location = nav.current_location;
            int current_level = nav.current_level;
            bool navigator_breack = nav.navigator_breack;
            nav = new Navigator() { current_location = current_location, current_level = current_level, navigator_breack = navigator_breack };
            spawn = 0;
            creatures = new List<Creature>();
            blocks = new List<Block>();
            backgrounds = new List<Background>();
            mario.nam = new List<int>();
            mario.we_stand = -1;
            mario.X = 83;
            mario.Y = 845 - 166;
            mario.direction = true;
            mario.TimerLeft = false;
            mario.TimerRight = false;
            mario.TimerGravity = false;
            mario.TimerSliding = false;
            mario.TimerSpace = false;
            mario.g = 1;
            mario.spaceG_max = 15;
            mario.spaceG = 20;
            mario.stopForm1_KeyDown = true;
            mario.run_animation = 0;
            mario.speed = 1;
            mario.Defines_the_image("Mario/Super Mario/Fiery Mario");
            mario.top = 3000;
            Spawn_Load();
            AnimationManager.PlayAnimation(
                                durationMs: 800,
                                intervalMs: 5,
                                onFrame: frame =>
                                {
                                    // Логика анимации (каждый тик таймера)
                                    if (frame < mario.height / 2 + mario.height % 2)
                                    {
                                        mario.Y -= 2; // Подбрасываем вверх
                                    }
                                    else
                                    {
                                        AnimationManager._currentFrame = AnimationManager._maxFrames;
                                    }
                                },
                                onComplete: () =>
                                {
                                    mario.stopForm1_KeyDown = false; // Разблокируем ввод
                                    mario.TimerGravity = true;
                                }
                            );
        }

        private void Dead_mario_restart()
        {
            spawn = 0;
            creatures = new List<Creature>();
            blocks = new List<Block>();
            backgrounds = new List<Background>();
            mario.nam = new List<int>();
            int level = nav.current_level;
            nav = new Navigator();
            if (lives > 0)
            {
                int coin = mario.coin;
                mario = new Mario(100, 595, nav.base_height_ordinary("ordinary"), nav.base_width_ordinary);
                mario.coin = coin;
                lives -= 1;
                nav.current_location = level;
                nav.current_level = level;
                Spawn_Load();
            }
            else GameOver();
        }

        private void GameOver()
        {
            mario = null;
            gameTimer.Stop();
            BackColor = Color.Black;
            BackgroundImage = Properties.Resources.gameover;
            BackgroundImageLayout = ImageLayout.Center;
            AnimationManager.PlayAnimation(
                                durationMs: 800,
                                intervalMs: 5,
                                onFrame: frame =>
                                {
                                },
                                onComplete: () =>
                                {
                                    BackgroundImage = null;
                                    Menu();
                                }
                            );
        }

        private void Spawn_Load()
        {
            bool exit = false;
            for (int i = 0; i < nav.navigator[nav.current_location].Count; i++)
            {
                object[] location_object = nav.navigator[nav.current_location][i];
                switch ((string)location_object[0])
                {
                    case "Block":
                        if((int)location_object[1] <= spawn + screenWidth + 700)
                        {
                            if ((string)location_object[7] != "between_locations" & (string)location_object[7] != "exit_from_the_location" & (string)location_object[7] != "return_between_locations")
                                blocks.Add(new Block(x: (int)location_object[1] - spawn, y: (int)location_object[2], name: (string)location_object[3], width: (int)location_object[4], height: (int)location_object[5], image: (Image)location_object[6], property: (string)location_object[7]));
                            else blocks.Add(new Block(x: (int)location_object[1] - spawn, y: (int)location_object[2], name: (string)location_object[3], width: (int)location_object[4], height: (int)location_object[5], image: (Image)location_object[6], property: (string)location_object[7], location_transfer: (int)location_object[8]));
                        }
                        else if (nav.navigator_breack == true) { nav.navigator_breack = false; exit = true; }
                        break;
                    case "Creature":
                        if ((int)location_object[1] <= spawn + screenWidth)
                        {
                            creatures.Add(new Creature(x: (int)location_object[1] - spawn, y: (int)location_object[2], image: (Image)location_object[3], direction: (bool)location_object[4], name: (string)location_object[5], width: (int)location_object[6], height: (int)location_object[7], condition: (List<string>)location_object[8], property: (string)location_object[9], g: (int)location_object[10], spaceG: (int)location_object[11], top: (int)location_object[12]));
                        }
                        else if (nav.navigator_breack == true) { nav.navigator_breack = false; exit = true; }
                        break;
                    case "Background":
                        if ((int)location_object[1] <= spawn + screenWidth)
                            backgrounds.Add(new Background(x: (int)location_object[1] - spawn, y: (int)location_object[2], name: (string)location_object[3], image: (Image)location_object[4]));
                        else if (nav.navigator_breack == true) { nav.navigator_breack = false; exit = true; }
                        break;
                    case "Settings":
                        if (nav.floor_parameters_are_met)
                        {
                            BackColor = (Color)location_object[1];
                            nav.end_location_X = (int)location_object[2];
                            nav.floor_parameters_are_met = false;
                        }
                        else if (nav.navigator_breack == true) { nav.navigator_breack = false; exit = true; }
                        break;
                    default: 
                        break;
                }
                if (exit) break;
                nav.navigator_breack = true;
                    if ((string)location_object[0] != "Settings")
                        nav.navigator[nav.current_location].RemoveAt(i--);
            }
        }
    }
}