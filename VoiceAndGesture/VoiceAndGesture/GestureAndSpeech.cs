using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    class GestureAndSpeech
    {
        public bool[] follow= {false,false};
        public bool[] come = { false, false, false};
        public bool[] sync = { false};

        public GestureAndSpeech()
        {
        }
        public void modifyFollow(int Dog, bool param)
        {
            follow[Dog] = param;
        }
        public void modifyCome(int Dog, bool param)
        {
            come[Dog] = param;
        }
        public void syncGest(int Dog, bool param)
        {
            sync[Dog] = param;
        }

    }
}
