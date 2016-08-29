using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.Processors
{
    public class ArToolkitProcessor : IProcessor
    {
        private int _id = -1;

        public void Deregister(int pipelineId)
        {
            ImageProcessing.RemoveProcessor(pipelineId, _id);
        }

        public void Register(int pipelineId)
        {
            _id = ImageProcessing.AddArToolkitProcessor(pipelineId);
            _id = -1;
        }
    }
}
