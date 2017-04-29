namespace Microsoft.Samples.Kinect.SpeechBasics
{
    using Microsoft.Kinect;
    using Microsoft.Speech.Recognition;
    using Speech.AudioFormat;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        enum dog { Pingo, Max, both }

        /// <summary> Active Kinect sensor </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Stream for 32b-16b conversion.
        /// </summary>
        private KinectAudioStream convertStream = null;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        private SpeechRecognitionEngine speechEngine = null;


        /// ********Handling the speech recognized event
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.5;
            Console.WriteLine(e.Result.Semantics.Value.ToString());
            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                if (e.Result.Semantics.Value.ToString() == "FOLLOW PINGO")
                {
                    GestureAndSpeech.follow[(int)dog.Pingo] = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "FOLLOW MAX")
                {
                    GestureAndSpeech.follow[(int)dog.Max] = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "FETCH PINGO")
                {
                    GestureAndSpeech.fetch[(int)dog.Pingo] = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "FETCH MAX")
                {
                    GestureAndSpeech.fetch[(int)dog.Max] = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "BARK PINGO")
                {
                    GestureAndSpeech.bark[(int)dog.Pingo] = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "BARK MAX")
                {
                    GestureAndSpeech.bark[(int)dog.Max] = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "COME PINGO")
                {
                    GestureAndSpeech.come[(int)dog.Pingo] = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "COME MAX")
                {
                    GestureAndSpeech.come[(int)dog.Max] = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "COME")
                {
                    GestureAndSpeech.come[(int)dog.both] = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "SYNCHRONIZE")
                {
                    GestureAndSpeech.sync = true;
                }
                else if (e.Result.Semantics.Value.ToString() == "PLAY PINGO")
                {
                    Server.sendClient1("play");
                }
                else if (e.Result.Semantics.Value.ToString() == "PLAY MAX")
                {
                    Server.sendClient2("play");
                }
                else if (e.Result.Semantics.Value.ToString() == "REST")
                {
                    for (int i = 0; i < 2; i++)
                    {
                        GestureAndSpeech.follow[i] = false;
                        GestureAndSpeech.come[i] = false;
                        GestureAndSpeech.bark[i] = false;
                        GestureAndSpeech.fetch[i] = false;
                    }
                    GestureAndSpeech.come[2] = false;
                    GestureAndSpeech.bark[2] = false;
                    GestureAndSpeech.sync = false;
                    Server.sendClient1("stop");
                    Server.sendClient2("stop");
                }
            }
        }


        private static RecognizerInfo TryGetKinectRecognizer()
        {
            IEnumerable<RecognizerInfo> recognizers;

            // This is required to catch the case when an expected recognizer is not installed.
            // By default - the x86 Speech Runtime is always expected. 
            try
            {
                recognizers = SpeechRecognitionEngine.InstalledRecognizers();
            }
            catch (COMException)
            {
                return null;
            }

            foreach (RecognizerInfo recognizer in recognizers)
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //  Server.StartListening();
            // Only one sensor is supported
            this.kinectSensor = KinectSensor.GetDefault();
            if (this.kinectSensor != null)
            {
                // open the sensor
                this.kinectSensor.Open();

                // grab the audio stream
                IReadOnlyList<AudioBeam> audioBeamList = this.kinectSensor.AudioSource.AudioBeams;
                System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();

                // create the convert stream
                this.convertStream = new KinectAudioStream(audioStream);
            }
            else
            {
                // on failure, set the status text
                Console.WriteLine("fAILURE");
            }

            RecognizerInfo ri = TryGetKinectRecognizer();

            if (null != ri)
            {

                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                // Create a grammar from grammar definition XML file.
                using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
                {
                    var g = new Grammar(memoryStream);
                    this.speechEngine.LoadGrammar(g);
                    //Console.Write(g.ToString());
                    // Console.Write("abc\n");
                }

                this.speechEngine.SpeechRecognized += this.SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected += this.SpeechRejected;

                // let the convertStream know speech is going active
                this.convertStream.SpeechActive = true;

                // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model. 
                // This will prevent recognition accuracy from degrading over time.
                ////speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                this.speechEngine.SetInputToAudioStream(
                    this.convertStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                this.speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                Console.WriteLine("No speech Recognized");
            }
        }

        /// <summary> Array for the bodies (Kinect will track up to 6 people simultaneously) </summary>
        private Body[] bodies = null;

        private Server serve = new Server();

        /// <summary> Reader for body frames </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary> Current status text to display </summary>
        private string statusText = null;

        /// <summary> KinectBodyView object which handles drawing the Kinect bodies to a View box in the UI </summary>
        private KinectBodyView kinectBodyView = null;

        /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
        private List<GestureDetector> gestureDetectorList = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        /// 
        public MainWindow()
        {

            // only one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();


            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // set the BodyFramedArrived event notifier
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            // initialize the BodyViewer object for displaying tracked bodies in the UI
            this.kinectBodyView = new KinectBodyView(this.kinectSensor);

            // initialize the gesture detection objects for our gestures
            this.gestureDetectorList = new List<GestureDetector>();

            // initialize the MainWindow
            this.InitializeComponent();

            // set our data context objects for display in UI
            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;

            // create a gesture detector for each body (6 bodies => 6 detectors) and create content controls to display results in the UI
            int col0Row = 0;
            GestureResultView result = new GestureResultView(0, false, false, 0.5f);
            GestureDetector detector = new GestureDetector(this.kinectSensor, result);
            this.gestureDetectorList.Add(detector);

            // split gesture results across the first two columns of the content grid
            ContentControl contentControl = new ContentControl();
            contentControl.Content = this.gestureDetectorList[0].GestureResultView;

            // Gesture results for bodies: 0, 2, 4
            Grid.SetColumn(contentControl, 0);
            Grid.SetRow(contentControl, col0Row);
            ++col0Row;

            this.contentGrid.Children.Add(contentControl);
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("Speech Rejected");
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.FrameArrived -= this.Reader_BodyFrameArrived;
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.gestureDetectorList != null)
            {
                // The GestureDetector contains disposable members (VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader)
                foreach (GestureDetector detector in this.gestureDetectorList)
                {
                    detector.Dispose();
                }

                this.gestureDetectorList.Clear();
                this.gestureDetectorList = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.IsAvailableChanged -= this.Sensor_IsAvailableChanged;
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
            if (null != this.convertStream)
            {
                this.convertStream.SpeechActive = false;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= this.SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= this.SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }
            //Server.destructClient1();
            // Server.destructClient2();
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            Console.WriteLine("sensor is available changed");
        }

        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }
            if (dataReceived)
            {
                // visualize the new body data
                this.kinectBodyView.UpdateBodyFrame(this.bodies);

                int maxBodies = 1;
                // we may have lost/acquired bodies, so update the corresponding gesture detectors
                if (this.bodies != null)
                {
                    for (int i = 0; i < maxBodies; i++)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;
                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }
                }
            }
        }
    }




}
