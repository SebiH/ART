using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.Outputs
{
    public class JsonOutput : IOutput
    {
        private int _id = -1;
        private ImageProcessing.JsonCallback _handler;

        public JsonOutput(ImageProcessing.JsonCallback handler)
        {
            _handler = handler;
        }

        public void Register(int pipelineId)
        {
            _id = ImageProcessing.AddJsonOutput(pipelineId, _handler);
        }

        public void Deregister(int pipelineId)
        {
            ImageProcessing.RemoveOutput(pipelineId, _id);
            _id = -1;
        }
    }
}
