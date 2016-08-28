using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.Outputs
{
    public interface IOutput
    {
        void Register(int pipelineId);
        void Deregister(int pipelineId);
    }
}
