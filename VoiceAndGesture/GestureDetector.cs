//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;
    using System;
    using System.Collections.Generic;

    public class GestureDetector : IDisposable
    {
        private readonly string gestureDatabase = @"Database/Hamza.gba";
        private readonly string gestureDatabase1 = @"Database/comeGesture.gba";
        private readonly string gestureDatabase2 = @"Database/followGest2.gba";
        private readonly string gestureDatabase3 = @"Database/syncGest.gba";

        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        private VisualGestureBuilderFrameReader vgbFrameReader = null;
        
        static string printNew;
        static string printOld = "";
        /// public bool comeSpeech, comeGesture, followSpeech, followGesture, syncSpeech, syncGesture;
        ///bool[] comeSpeech = new bool[3];
        

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
            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }
            // load the 'Seated' gesture from the gesture database
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(gestureDatabase))
            {
                // we could load all available gestures in the database with a call to vgbFrameSource.AddGestures(database.AvailableGestures), 
                // but for this program, we only want to track one discrete gesture from the database, so we'll load it by name
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    if (gesture.Name.Equals("Hamza"))
                    {
                        this.vgbFrameSource.AddGesture(gesture);

                    }
                }
                using (VisualGestureBuilderDatabase database1 = new VisualGestureBuilderDatabase(gestureDatabase1))
                {
                    foreach (Gesture gesture in database1.AvailableGestures)
                    {
                        if (gesture.Name.Equals("comeGesture"))
                        {
                            this.vgbFrameSource.AddGesture(gesture);

                        }
                    }

                }
                using (VisualGestureBuilderDatabase database2 = new VisualGestureBuilderDatabase(gestureDatabase2))
                {
                    foreach (Gesture gesture in database2.AvailableGestures)
                    {
                        if (gesture.Name.Equals("followGest2"))
                        {
                            this.vgbFrameSource.AddGesture(gesture);

                        }
                    }

                }
                using (VisualGestureBuilderDatabase database3 = new VisualGestureBuilderDatabase(gestureDatabase3))
                {
                    foreach (Gesture gesture in database3.AvailableGestures)
                    {
                        if (gesture.Name.Equals("syncGest"))
                        {
                            this.vgbFrameSource.AddGesture(gesture);

                        }
                    }

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

        //check result
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
                            if (gesture.Name.Equals("Hamza") || gesture.Name.Equals("followGest2") || gesture.Name.Equals("syncGest") || gesture.Name.Equals("comeGesture"))
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
                                            case "followGest2":
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
                                            case "comeGesture":
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
                                            case "syncGest":
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
