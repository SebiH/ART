using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.Processors
{
    public interface IProcessor
    {
        void Register(int pipelineId);
        void Deregister(int pipelineId);
    }
}
