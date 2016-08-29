using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.Outputs
{
    public class DirectXOutput : IOutput
    {
        public enum Eye
        {
            Left = 0,
            Right = 1
        }

        private int _id;
        private Eye _eye;
        private IntPtr _texturePtr;

        public DirectXOutput(IntPtr texturePtr, Eye eye)
        {
            _texturePtr = texturePtr;
            _eye = eye;
            _id = -1;
        }

        public void Register(int pipelineId)
        {
            _id = ImageProcessing.RegisterUnityPointer(pipelineId, (int)_eye, _texturePtr);
        }

        public void Deregister(int pipelineId)
        {
            ImageProcessing.RemoveOutput(pipelineId, _id);
            _id = -1;
        }
    }
}
