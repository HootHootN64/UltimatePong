using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PingPong
{
    enum Speeds 
    { 
        DEBUG = 10
    }


    public partial class MainWindow : Window
    {
        //VARS
        double newWindowWidth;
        double newWindowHeight;
        double mouseX, mouseY;
        int time = 0;
        int recTime = 0, recOnce = 0;
        int speedX, speedY, initSpeed = (int) Speeds.DEBUG;
        bool pause = false;

        bool readSuccess;

        static SoundPlayer audio;

        public MainWindow()
        {
            //INIT
            InitializeComponent();
            SpawnBall();
            SpawnPlayer();
            recOnce = 0;

            //SOUND
            audio = new SoundPlayer(PingPong2.Properties.Resources.start); 
            audio.Play();

            //SoundPlayer audio2 = new SoundPlayer(PingPong2.Properties.Resources.bgm3);
            //audio2.PlayLooping();


            //LOAD SAVE
            if (File.Exists("Highscore.txt"))
            {
                readSuccess = Int32.TryParse(File.ReadAllText("Highscore.txt"), out recTime);

                if(readSuccess)
                    ScoreRec.Content = "(" + recTime + ")";
            }
            else 
            {
                File.WriteAllText("Highscore.txt", "0");
            }

            //CURSOR
            Cursor = Cursors.None;

            //WATCHDOGS
            this.MouseLeftButtonDown += OnMouseClickLeft;
            this.MouseRightButtonDown += OnMouseClickRight;
                //if (pause == true) return; //DOESN'T WORK HERE
            this.SizeChanged += OnWindowSizeChanged;
            this.MouseMove += OnMouseMove;

            //TIMER 1 SECONDS
            System.Timers.Timer secTime = new System.Timers.Timer();
            secTime.Elapsed += new ElapsedEventHandler(SecondsTimer);
            secTime.Interval = 1000;
            secTime.Enabled = true;

            //TIMER 0.1 SECONDS
            System.Timers.Timer ballTime = new System.Timers.Timer();
            ballTime.Elapsed += new ElapsedEventHandler(BallTimer);
            ballTime.Interval = 100;
            ballTime.Enabled = true;
        }

        //INIT
        protected void SpawnBall()
        {
            Random rnd = new Random();
            int rand = rnd.Next(1, 799);

            Ball.Margin = new Thickness(rand, 50, 0, 0);

            speedX = speedY = initSpeed;
        }

        protected void SpawnPlayer()
        {
            Player.Margin = new Thickness(400-Player.Width/2, 434 - Player.Height, 0, 0);
        }

        //MOVE
        protected void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (pause == true) return;

            //GET MOUSE POS
            System.Windows.Point position = e.GetPosition(this);
            mouseX = position.X;
            mouseY = position.Y;

            //GET OLD PLAYER POS
            double oldPlayX = Player.Margin.Left;
            double oldPlayY = Player.Margin.Top;

            //SET NEW PLAYER POS
            if (mouseX < Bg.Width - Player.Width)
                Player.Margin = new Thickness(mouseX, oldPlayY, 0, 0);
        }

        protected void BallTimer(object sender, ElapsedEventArgs e)
        {
            if (pause == true) return;

            Dispatcher.Invoke(new Action(() =>
            {
                //CHANGE DIR
                if (Ball.Margin.Left <= 0) //HIT LEFT
                { 
                    speedX *= -1;

                    audio = new SoundPlayer(PingPong2.Properties.Resources.reload2);
                    audio.Play();
                }
                else if(Ball.Margin.Left + Ball.Width >= 800) //HIT RIGHT
                {
                    speedX *= -1;

                    audio = new SoundPlayer(PingPong2.Properties.Resources.reload2);
                    audio.Play();
                }

                if (Ball.Margin.Top <= 0) //HIT TOP
                {
                    speedY *= -1;

                    audio = new SoundPlayer(PingPong2.Properties.Resources.reload2);
                    audio.Play();
                }
                else if (Ball.Margin.Top + Ball.Height >= 434 - Player.Height) 
                {
                    if (Ball.Margin.Left >= Player.Margin.Left && 
                    Ball.Margin.Left <= Player.Margin.Left + Player.Width) //HIT PLAYER -> GAIN SPEED
                    {
                        speedY *= -1;

                        speedX = (speedX < 0) ? speedX -2 : speedX +2;
                        speedY = (speedY < 0) ? speedY -2 : speedY +2;

                        audio = new SoundPlayer(PingPong2.Properties.Resources.shot);
                        audio.Play();
                    }
                    else if (Ball.Margin.Top + Ball.Height >= 434) //HIT BOTTOM -> RESET
                    {
                        SpawnBall();

                        if (time > recTime) //SAVE RECORD
                        {
                            recTime = time;
                            ScoreRec.Content = "(" + recTime + ")";
                            File.WriteAllText("Highscore.txt", recTime.ToString());
                        }

                        audio = new SoundPlayer(PingPong2.Properties.Resources.die);
                        audio.Play();

                        time = 0;
                        recOnce = 0;
                    }
                }

                double bX = Ball.Margin.Left + speedX;
                double bY = Ball.Margin.Top + speedY;

                //CHECK MIN MAX BOUNDS...
                     if (bX < 0)    bX = 0;
                else if (bX > 800)  bX = 800 - Ball.Width;

                     if (bY < 0)    bY = 0;
                else if (bY > 434)  bY = 434 - Ball.Height;

                //...THEN MOVE
                Ball.Margin = new Thickness(bX, bY, 0, 0);

                //DEBUG
                Debug1.Content = Ball.Margin.Left;
                Debug2.Content = Ball.Margin.Top;
                Debug3.Content = Player.Margin.Left;
                Debug4.Content = Player.Margin.Top;
                Debug5.Content = speedX;
                Debug6.Content = speedY;
                Debug7.Content = newWindowHeight;
                Debug8.Content = newWindowWidth;
                Debug9.Content = mouseX;
                Debug10.Content = mouseY;
                Debug11.Content = pause;
            }));
        }

        protected void SecondsTimer(object sender, ElapsedEventArgs e)
        {
            if (pause == true) return;

            Dispatcher.Invoke(new Action(() =>
            {
                time++;
                Score.Content = time;

                if(time > recTime && recTime != 0 && recOnce == 0) 
                {
                    audio = new SoundPlayer(PingPong2.Properties.Resources.hiscore);
                    audio.Play();

                    recOnce = 1;
                }
            }));
        }

        //WINDOW CHANGE
        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            newWindowHeight = e.NewSize.Height;
            newWindowWidth = e.NewSize.Width;
            double prevWindowHeight = e.PreviousSize.Height;
            double prevWindowWidth = e.PreviousSize.Width;
        }

        //PAUSE
        protected void OnMouseClickLeft(object sender, MouseEventArgs e)
        {
            if (pause == true)
            {
                pause = false;
                audio = new SoundPlayer(PingPong2.Properties.Resources.unpause);
            }

            else
            {
                pause = true;
                audio = new SoundPlayer(PingPong2.Properties.Resources.pause);
            }

            audio.Play();
        }

        //DEBUG
        protected void OnMouseClickRight(object sender, MouseEventArgs e)
        {
            if (Debug1.IsVisible)
            {
                Debug1.Visibility = Visibility.Hidden;
                Debug2.Visibility = Visibility.Hidden;
                Debug3.Visibility = Visibility.Hidden;
                Debug4.Visibility = Visibility.Hidden;
                Debug5.Visibility = Visibility.Hidden;
                Debug6.Visibility = Visibility.Hidden;
                Debug7.Visibility = Visibility.Hidden;
                Debug8.Visibility = Visibility.Hidden;
                Debug9.Visibility = Visibility.Hidden;
                Debug10.Visibility = Visibility.Hidden;
                Debug11.Visibility = Visibility.Hidden;
                Lab1.Visibility = Visibility.Hidden;
                Lab2.Visibility = Visibility.Hidden;
                Lab3.Visibility = Visibility.Hidden;
                Lab4.Visibility = Visibility.Hidden;
                Lab5.Visibility = Visibility.Hidden;
                Lab6.Visibility = Visibility.Hidden;

                audio = new SoundPlayer(PingPong2.Properties.Resources.debugger);
                audio.Play();
            }
            else
            {
                Debug1.Visibility = Visibility.Visible;
                Debug2.Visibility = Visibility.Visible;
                Debug3.Visibility = Visibility.Visible;
                Debug4.Visibility = Visibility.Visible;
                Debug5.Visibility = Visibility.Visible;
                Debug6.Visibility = Visibility.Visible;
                Debug7.Visibility = Visibility.Visible;
                Debug8.Visibility = Visibility.Visible;
                Debug9.Visibility = Visibility.Visible;
                Debug10.Visibility = Visibility.Visible;
                Debug11.Visibility = Visibility.Visible;
                Lab1.Visibility = Visibility.Visible;
                Lab2.Visibility = Visibility.Visible;
                Lab3.Visibility = Visibility.Visible;
                Lab4.Visibility = Visibility.Visible;
                Lab5.Visibility = Visibility.Visible;
                Lab6.Visibility = Visibility.Visible;
            }
        }
    }
}
