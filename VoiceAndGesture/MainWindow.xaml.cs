namespace Microsoft.Samples.Kinect.SpeechBasics
{
    using Microsoft.Kinect;
    using Microsoft.Speech.Recognition;
    using Speech.AudioFormat;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        enum dog { Pingo, Max, both }

        private KinectSensor kinectSensor = null;

        private KinectAudioStream convertStream = null;

        private SpeechRecognitionEngine speechEngine = null;

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
                    Server.sendClient1("fetch");
                }
                else if (e.Result.Semantics.Value.ToString() == "FETCH MAX")
                {
                    GestureAndSpeech.fetch[(int)dog.Max] = true;
                    Server.sendClient2("fetch");
                }
                else if (e.Result.Semantics.Value.ToString() == "FETCH")
                {
                    GestureAndSpeech.come[(int)dog.both] = true;
                    Server.sendClient1("fetch");
                    Server.sendClient2("fetch");
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
                //else if (e.Result.Semantics.Value.ToString() == "FINAL")
                //{
                //    Server.sendClient1("end");
                //    Server.sendClient2("end");
                //}
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
            Server.StartListening();
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

        private Body[] bodies = null;

        private Server serve = new Server();

        private BodyFrameReader bodyFrameReader = null;

        private string statusText = null;

        private KinectBodyView kinectBodyView = null;

        private List<GestureDetector> gestureDetectorList = null;

        public MainWindow()
        {

            this.kinectSensor = KinectSensor.GetDefault();

            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            this.kinectSensor.Open();

            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            this.kinectBodyView = new KinectBodyView(this.kinectSensor);

            this.gestureDetectorList = new List<GestureDetector>();

            this.InitializeComponent();

            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;

            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;

            for (int i = 0; i < maxBodies; ++i)
            {
                GestureResultView result = new GestureResultView(i, false, false, 0.0f);
                GestureDetector detector = new GestureDetector(this.kinectSensor, result);

                this.gestureDetectorList.Add(detector);

                ContentControl contentControl = new ContentControl();
                contentControl.Content = this.gestureDetectorList[i].GestureResultView;

                this.contentGrid.Children.Add(contentControl);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

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

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("Speech Rejected");
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived -= this.Reader_BodyFrameArrived;
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.gestureDetectorList != null)
            {
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
            Server.destructClient1();
            Server.destructClient2();
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
                this.kinectBodyView.UpdateBodyFrame(this.bodies);

                int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
                if (this.bodies != null)
                {
                    for (int i = 0; i < maxBodies; i++)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }
                }
            }
        }
    }
    
}
