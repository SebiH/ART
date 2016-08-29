using Assets.Modules.Vision.Outputs;
using Assets.Modules.Vision.Processors;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Vision
{
    public class Pipeline : MonoBehaviour
    {
        private int _id;
        private List<IProcessor> _processors = new List<IProcessor>();
        private List<IOutput> _outputs = new List<IOutput>();

        void Awake()
        {
            _id = ImageProcessing.CreatePipeline();
        }

        void OnDestroy()
        {
            ImageProcessing.RemovePipeline(_id);
        }


        public void AddProcessor(IProcessor processor)
        {
            _processors.Add(processor);
            processor.Register(_id);
        }

        public void RemoveProcessor(IProcessor processor)
        {
            _processors.Remove(processor);
            processor.Deregister(_id);
        }

        public void AddOutput(IOutput output)
        {
            _outputs.Add(output);
            output.Register(_id);
        }

        public void RemoveOutput(IOutput output)
        {
            _outputs.Remove(output);
            output.Deregister(_id);
        }
    }
}
