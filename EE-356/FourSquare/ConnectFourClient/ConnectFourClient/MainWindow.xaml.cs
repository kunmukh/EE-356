﻿using System;
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
using System.ComponentModel;
using System.Net.Sockets;   //include sockets class
using System.Net;  //needed for type IPAddress
using System.IO;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace ConnectFourClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Socket connection variables
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;
        delegate void SetTextCallback(String text);
        BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private String UserName = "";
        
        //Game setup logic
        private Boolean Player1 = false;
        private static int height = 6, width = 7;
        private Cell[,] grid = new Cell[height, width];
        private TranslateTransform animatedTranslateTransform;
        Storyboard pathAnimationStoryboard;
        System.Media.SoundPlayer myPlayerW;      
        //delegate function so that chat, graphics , image can be shown
        delegate void SetBitMapCallbCk(RenderTargetBitmap bmp);
        delegate void SetStartBtn(Boolean b);
        delegate void SetChatBtn(Boolean b);
        delegate void SetImg(Boolean b);
        delegate void SetCnv(int startIndex1, int endIndex1, int startIndex2, int endIndex2);       
        Boolean blue;

        public MainWindow()
        {
            InitializeComponent();

            //canvas
            cnv1.ClipToBounds = true;
            // Create a transform. This transform
            // will be used to move the rectangle.
            animatedTranslateTransform =
                new TranslateTransform();

            // Register the transform's name with the page
            // so that they it be targeted by a Storyboard.
            cnv1.RegisterName("AnimatedTranslateTransform", animatedTranslateTransform);
            //stop the client to send a message even when the client is not connected
            btn_Send.IsEnabled = false;
        }

        //sending a text message
        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            sw.WriteLine("Chat\n" + UserName + txtbxMessage.Text);
            sw.Flush();
            txtbxMessage.Text = "";
        }

        //starts the server and connects
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //enable the chat button if the chat is succesfully connected
            btn_Send.IsEnabled = true;
            //get the username
            UserName = txtbxUsername.Text + ">> ";
            TcpClient newcon = new TcpClient();
            newcon.Connect("127.0.0.1", 9090);  //IPAddress of Server
            ns = newcon.GetStream();
            sr = new StreamReader(ns);  //Stream Reader and Writer take away some of the overhead of keeping track of Message size.  By Default WriteLine and ReadLine use Line Feed to delimit the messages
            sw = new StreamWriter(ns);

            //create the background worker and add it
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerAsync("Message to Worker");
            txtbxUsername.IsEnabled = false;

            //generate an empty board
            //draw the board
            //no more server connection can be made after connection
            makeEmptyBoard();
            drawBoard();
            changeStartBtn(false);
        }

        //get the column postion from the button click
        private void imgGame_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(imgGame);
            int x, y;

            x = (int)point.X;
            y = (int)point.Y;

            if (x > 0 && x < 231 && y > 33 && y < 231)
            {
                //seelct the appriopiate row and col                
                int colSel = x / 33;
                sw.WriteLine("Move\n" + colSel);
                sw.Flush();
            }
        }

        //move the coin as the mouse is moved over screen
        private void imgGame_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(imgGame);

            int x, y;

            x = (int)point.X;
            y = (int)point.Y;


            int rowSel = (y / 33) - 1;
            int colSel = x / 33;

            if (x > 0 && x < 231 && y > 33 && y < 198)
            {
                int gapx = colSel * 46;

                //lblMessage.Content = rowSel.ToString() + " " + colSel.ToString() + " " + gapx.ToString();

                Pen[] penArray = new Pen[2];
                penArray[0] = new Pen(Brushes.Black, 1);
                penArray[1] = new Pen(Brushes.Black, 1);

                Brush[] brushArray = new Brush[2];
                brushArray[1] = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); //red
                brushArray[0] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)); //blue

                DrawingVisual vis = new DrawingVisual();
                DrawingContext dc = vis.RenderOpen();

                int coinSize = 46, gapy = 23;


                Brush b = new SolidColorBrush();
                Pen p = new Pen();

                //select the brush and pen color depending on the player
                if (Player1)
                {
                    p = penArray[1];
                    b = brushArray[0];
                    dc.DrawEllipse(b, p, new Point(gapx + coinSize / 2, gapy), coinSize / 2, coinSize / 2);
                }
                else
                {
                    p = penArray[1];
                    b = brushArray[1];
                    dc.DrawEllipse(b, p, new Point(gapx + coinSize / 2, gapy), coinSize / 2, coinSize / 2);
                }

                dc.Close();
                RenderTargetBitmap bmp = new RenderTargetBitmap(1384, 1280, 300, 300, PixelFormats.Pbgra32);
                bmp.Render(vis);
                imgGame2.Source = bmp;
            }
        }

        //the background worker that does the work
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    string inputStream = sr.ReadLine();  //Note Read only reads into a byte array.  Also Note that Read is a "Blocking Function"     

                    //if it is a server message 
                    if (inputStream.Contains("Server"))
                    {
                        string newstring = sr.ReadLine();
                        InsertTextMessage(newstring);
                    }
                    //this is a chat message
                    if (inputStream.Contains("Chat"))
                    {
                        string newstring = sr.ReadLine();
                        InsertText(newstring);
                    }
                    //this is a move made
                    if (inputStream.Contains("Move"))
                    {
                        //play sound
                        String s = System.Environment.CurrentDirectory;
                        var myPlayer = new System.Media.SoundPlayer();
                        myPlayer.SoundLocation = System.IO.Path.Combine(System.Environment.CurrentDirectory, @"..\..\..\coin.wav");
                        myPlayer.Play();

                        //gets the row, column, color information from the message
                        string iSels = sr.ReadLine();
                        string colSels = sr.ReadLine();
                        string color = sr.ReadLine();

                        //converts the row,col to string
                        int iSel = Convert.ToInt32(iSels);
                        int colSel = Convert.ToInt32(colSels);

                        //selects the color
                        if (color == "blue")
                        {
                            grid[iSel, colSel].setCoinBlue();
                            blue = true;
                        }
                        else
                        {
                            grid[iSel, colSel].setCoinRed();
                            blue = false;
                        }

                        //drawBoard();
                        doAnimation(colSel, 0, colSel, iSel);
                    }
                    //get the player number from the server, if they are first or nor
                    if (inputStream.Contains("Player1"))
                    {
                        string status = sr.ReadLine();
                        if (status.Contains("true"))
                        {
                            Player1 = true;
                            //sends the respective message
                            InsertText("You are Blue. And YOU START!!!!");
                        }
                        else
                        {
                            Player1 = false;
                            //sends the respective message
                            InsertText("You are Red. BLUE STARTS, so PLEASE WAIT");
                        }
                        //cleasr teh board and draws the board
                        makeEmptyBoard();
                        drawBoard();

                    }
                    //if disconnected, closes the connection 
                    if (inputStream.Contains("Disconnect"))
                    {
                        sr.Close();
                        sw.Close();
                        changeStartBtn(false);
                        changeChatBtn(false);
                        changeImg(false);                        
                    }

                }
                catch
                {
                    //ns.Close();
                    //System.Environment.Exit(System.Environment.ExitCode); //close all                     
                }

            }

        }

        //delegate function to insert image
        private void InsertBitMap(RenderTargetBitmap bmp)
        {
            if (this.imgGame.Dispatcher.CheckAccess())
            {
                this.imgGame.Source = bmp;
            }
            else
            {
                bmp.Freeze();
                this.imgGame.Dispatcher.BeginInvoke(new SetBitMapCallbCk(InsertBitMap), bmp);                
            }
        }        

        //delegate function to insert text message from server
        private void InsertTextMessage(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.lblMessage.Dispatcher.CheckAccess())
            {
                this.lblMessage.Content = text; ;

            }
            else
            {
                lblMessage.Dispatcher.BeginInvoke(new SetTextCallback(InsertTextMessage), text);
            }
        }

        //insert text message into the list box
        private void InsertText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.listBox1.Dispatcher.CheckAccess())
            {
                this.listBox1.Items.Insert(0, text);

            }
            else
            {
                listBox1.Dispatcher.BeginInvoke(new SetTextCallback(InsertText), text);
            }
        }

        //chnages the start button to the sent boolean
        private void changeStartBtn(Boolean b)
        {            
            if (this.btnStart.Dispatcher.CheckAccess())
            {
                this.btnStart.IsEnabled = b;

            }
            else
            {
                btnStart.Dispatcher.BeginInvoke(new SetStartBtn(changeStartBtn), b);
            }
        }

        //enabling or disabling the chat button after disconnection
        private void changeChatBtn(Boolean b)
        {
            if (this.btn_Send.Dispatcher.CheckAccess())
            {
                this.btn_Send.IsEnabled = b;

            }
            else
            {
                btn_Send.Dispatcher.BeginInvoke(new SetChatBtn(changeChatBtn), b);
            }
        }

        //changes the image as per the current game
        private void changeImg(Boolean b)
        {
            if (this.imgGame.Dispatcher.CheckAccess())
            {
                this.imgGame.IsEnabled = b;

            }
            else
            {
                imgGame.Dispatcher.BeginInvoke(new SetImg(changeImg), b);
            }
        }

        //draws the board
        void drawBoard()
        {
            Pen[] penArray = new Pen[3];
            penArray[0] = new Pen(Brushes.Black, 1);
            penArray[1] = new Pen(Brushes.Black, 1);
            penArray[2] = new Pen(Brushes.Black, 2);


            Brush[] brushArray = new Brush[3];
            brushArray[1] = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); //red
            brushArray[0] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)); //blue
            brushArray[2] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));


            DrawingVisual vis = new DrawingVisual();
            DrawingContext dc = vis.RenderOpen();

            //draw the square
            int gapx = 0, gapy = 46, coinSize = 46;
            Brush b = new SolidColorBrush();
            Pen p = new Pen();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    p = penArray[2];
                    b = brushArray[2];
                    dc.DrawRectangle(b, p, new Rect(gapx, gapy, coinSize, coinSize));
                    gapx += coinSize;
                }
                gapy += coinSize;
                gapx = 0;
            }

            //draw the ellpise
            gapx = 0; gapy = coinSize = 46;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //if grid is not empty
                    if (!grid[i, j].isEmpty())
                    {
                        //if it is first player
                        if (grid[i, j].getCoin().getColor() == 0)
                        {
                            p = penArray[2];
                            b = brushArray[0];
                            dc.DrawEllipse(b, p, new Point(gapx + coinSize / 2, gapy + coinSize / 2), coinSize / 2, coinSize / 2);
                        }
                        else
                        {
                            p = penArray[2];
                            b = brushArray[1];
                            dc.DrawEllipse(b, p, new Point(gapx + coinSize / 2, gapy + coinSize / 2), coinSize / 2, coinSize / 2);
                        }
                    }

                    gapx += coinSize;
                }
                gapy += coinSize;
                gapx = 0;
            }


            //genrates the bitmap
            //and send the bitmap to the screen
            dc.Close();
            RenderTargetBitmap bmp = new RenderTargetBitmap(1384, 1280, 300, 300, PixelFormats.Pbgra32);
            bmp.Render(vis);
            InsertBitMap(bmp);
        }

        //creates an empty baord
        void makeEmptyBoard()
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    grid[i, j] = new Cell();
                }
            }
        }

        //chanage the player
        void changePlayer()
        {
            if (Player1)
            {
                Player1 = false;
            }
            else
            {
                Player1 = true;
            }
        }        

        //do the coin animation
        private void doAnimation(int startIndex1, int endIndex1, int startIndex2, int endIndex2)
        {
            if (this.cnv1.Dispatcher.CheckAccess())
            {
                int coinSize = 35;
                SolidColorBrush bluBrush;

                //select the apporipate color for the ellipse
                if (blue)
                {
                    bluBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                }
                else
                {
                    bluBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }

                Ellipse ell1 = new Ellipse();
                ell1.Height = coinSize;
                ell1.Width = coinSize;
                ell1.Fill = bluBrush;

                cnv1.Children.Add(ell1);

                ell1.RenderTransform = animatedTranslateTransform;

                ///*start
                // Create the animation path.
                PathGeometry animationPath = new PathGeometry();
                PathFigure pFigure = new PathFigure();

                PolyBezierSegment pBezierSegment = new PolyBezierSegment();

                Point start = new Point(startIndex1 * 33, endIndex1 * 33);
                Point end = new Point(startIndex2 * 33, (endIndex2 + 1) * 33);

                pFigure.StartPoint = start;
                pFigure.Segments.Add(pBezierSegment);

                Console.WriteLine("Point start: " + start + " Point end: " + end);
                LoadPathPoints(pBezierSegment, start, end);
                animationPath.Figures.Add(pFigure);

                // Freeze the PathGeometry for performance benefits.
                animationPath.Freeze();
                // Create a DoubleAnimationUsingPath to move the
                // rectangle horizontally along the path by animating 
                // its TranslateTransform.
                DoubleAnimationUsingPath translateXAnimation =
                    new DoubleAnimationUsingPath();
                translateXAnimation.PathGeometry = animationPath;
                translateXAnimation.Duration = TimeSpan.FromSeconds(1);

                // Set the Source property to X. This makes
                // the animation generate horizontal offset values from
                // the path information. 
                translateXAnimation.Source = PathAnimationSource.X;

                // Set the animation to target the X property
                // of the TranslateTransform named "AnimatedTranslateTransform".
                Storyboard.SetTargetName(translateXAnimation, "AnimatedTranslateTransform");
                Storyboard.SetTargetProperty(translateXAnimation,
                    new PropertyPath(TranslateTransform.XProperty));
                // Create a DoubleAnimationUsingPath to move the
                // rectangle vertically along the path by animating 
                // its TranslateTransform.
                DoubleAnimationUsingPath translateYAnimation =
                    new DoubleAnimationUsingPath();
                translateYAnimation.PathGeometry = animationPath;
                translateYAnimation.Duration = TimeSpan.FromSeconds(1);

                // Set the Source property to Y. This makes
                // the animation generate vertical offset values from
                // the path information. 
                translateYAnimation.Source = PathAnimationSource.Y;

                // Set the animation to target the Y property
                // of the TranslateTransform named "AnimatedTranslateTransform".
                Storyboard.SetTargetName(translateYAnimation, "AnimatedTranslateTransform");
                Storyboard.SetTargetProperty(translateYAnimation,
                    new PropertyPath(TranslateTransform.YProperty));

                // Create a Storyboard to contain and apply the animations.
                pathAnimationStoryboard = new Storyboard();

                pathAnimationStoryboard.Children.Add(translateXAnimation);
                pathAnimationStoryboard.Children.Add(translateYAnimation);
                // Start the animations.           
                pathAnimationStoryboard.Completed += new EventHandler(Story_Completed);
                pathAnimationStoryboard.Begin(cnv1, true);
            }
            else
            {
                cnv1.Dispatcher.BeginInvoke(new SetCnv(doAnimation), startIndex1, endIndex1, startIndex2, endIndex2);
            }

        }

        //after the animation is done, reprint the baord
        private void Story_Completed(object sender, EventArgs e)
        {
            cnv1.Children.Clear();            
            drawBoard();
        }

        //load the animation path
        private void LoadPathPoints(PolyBezierSegment pBezierSegment, Point start, Point end)
        {
            double incrx = (end.X - start.X) / 5;
            double incry = (end.Y - start.Y) / 5;
            int i;
            double x, y;
            x = start.X; y = start.Y;
            for (i = 0; i < 6; i++)
            {
                pBezierSegment.Points.Add(new Point(x, y));
                x += incrx;
                y += incry;
            }
        }

    }

    //class cell that has the attribute of being last, empty 
    public class Cell
    {
        public Coin _c { get; set; }
        public bool _Empty { get; set; }

        public Cell()
        {
            //create a empty cell
            _c = new Coin(2);
            _Empty = true;
        }

        public Cell(bool empty, int coinColor)
        {
            //creates a cell with a specif color
            _Empty = empty;
            _c = new Coin(coinColor);
        }

        public void setCoinColor(Coin co)
        {
            _c.setColor(co.getColor());
            _Empty = false;
        }

        public void setCoinBlue()
        {
            _c.setColor(0);
            _Empty = false;
        }

        public void setCoinRed()
        {
            _c.setColor(1);
            _Empty = false;
        }

        public void setEmpty(bool status)
        {
            _Empty = status;
        }

        public bool isEmpty()
        {
            return _Empty;
        }

        public Coin getCoin()
        {
            return _c;
        }

        public override string ToString()
        {
            if (_Empty)
            {
                return " 0";
            }
            else
            {
                return " " + _c.getColor().ToString();
            }

        }
    }

    //class coin that has the attribute of color
    public class Coin
    {
        public int _Color { get; set; }

        public Coin()
        {
            //makes the coin of blue color
            _Color = 0;
        }

        public Coin(int color)
        {
            //makes the coin of speciif color
            _Color = color;
        }

        public int getColor()
        {
            //return the color of coin
            return _Color;
        }

        public void setColor(int color)
        {
            //sets the color of coin
            _Color = color;
        }

    }
}
