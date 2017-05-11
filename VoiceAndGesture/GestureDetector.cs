namespace Microsoft.Samples.Kinect.SpeechBasics
{
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;
    using System;
    using System.Collections.Generic;

    public class GestureDetector : IDisposable
    {
        private readonly string gestureDatabase = @"Database/finale.gbd";

        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        static string printNew;
        static string printOld = "";

        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            if (gestureResultView == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }

            this.GestureResultView = gestureResultView;
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(gestureDatabase))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    this.vgbFrameSource.AddGesture(gesture);
                }
            }
        }
        public GestureResultView GestureResultView { get; private set; }

        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                    if (discreteResults != null)
                    {
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.Name.Equals("come") || gesture.Name.Equals("follow") || gesture.Name.Equals("sync"))
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);


                                if (result != null)
                                {
                                    this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);

                                    if (result.Detected)
                                    {
                                        switch (gesture.Name)
                                        {
                                             case "follow":
                                                 {
                                                     if (GestureAndSpeech.follow[0])
                                                     {
                                                         printNew = "follow pingo speech & gesture";
                                                         if (printNew != printOld)
                                                         {
                                                             Server.sendClient1("follow");
                                                             Console.WriteLine(printNew);
                                                         }
                                                         printOld = printNew;
                                                     }
                                                     else if (GestureAndSpeech.follow[1])
                                                     {
                                                         printNew = "follow max speech & gesture";
                                                         if (printNew != printOld)
                                                         {
                                                             Server.sendClient2("follow");
                                                             Console.WriteLine(printNew);
                                                         }
                                                         printOld = printNew;
                                                     }
                                                 }
                                                 break;
                                             case "fetchGest":
                                                 {
                                                     if (GestureAndSpeech.fetch[0])
                                                     {
                                                         printNew = "fetch pingo speech & gesture";
                                                         if (printNew != printOld)
                                                         {
                                                             Server.sendClient1("fetch");
                                                             Console.WriteLine(printNew);
                                                         }
                                                         printOld = printNew;
                                                     }
                                                     else if (GestureAndSpeech.fetch[1])
                                                     {
                                                         printNew = "fetch max speech & gesture";
                                                         if (printNew != printOld)
                                                         {
                                                             Server.sendClient2("fetch");
                                                             Console.WriteLine(printNew);
                                                         }
                                                         printOld = printNew;
                                                     }
                                                 }
                                                 break;
                                             case "come":
                                                 {
                                                     if (GestureAndSpeech.come[0])
                                                     {
                                                         printNew = "come pingo speech & gesture";
                                                         if (printNew != printOld)
                                                         {
                                                             Server.sendClient1("come");
                                                             Console.WriteLine(printNew);
                                                         }
                                                         printOld = printNew;
                                                     }
                                                     else if (GestureAndSpeech.come[1])
                                                     {
                                                         printNew = "come max speech & gesture";
                                                         if (printNew != printOld)
                                                         {
                                                             Server.sendClient2("come");
                                                             Console.WriteLine(printNew);
                                                         }
                                                         printOld = printNew;
                                                     }
                                                     else if (GestureAndSpeech.come[2])
                                                     {
                                                         printNew = "come both speech & gesture";
                                                         if (printNew != printOld)
                                                         {
                                                             Server.sendClient1("come");
                                                             Server.sendClient2("come");
                                                             Console.WriteLine(printNew);
                                                         }
                                                         printOld = printNew;
                                                     }
                                                 }
                                                 break;
                                             case "sync":
                                                 {
                                                     if (GestureAndSpeech.sync)
                                                     {
                                                         printNew = "sync both speech & gesture";
                                                         if (printNew != printOld)
                                                         {
                                                             Server.sendClient1("sync");
                                                             Server.sendClient2("sync");
                                                             Console.WriteLine(printNew);
                                                         }
                                                         printOld = printNew;
                                                     }
                                                 }
                                                 break;
                                            default: break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            // update the GestureResultView object to show the 'Not Tracked' image in the UI
            this.GestureResultView.UpdateGestureResult(false, false, 0.0f);
        }
    }
}