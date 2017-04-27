using Assets.Modules.Vision.Processors;
using UnityEngine;

namespace Assets.Modules.Vision
{
    public class UndistortImage : MonoBehaviour
    {
        public Pipeline VisionPipeline;
        private UndistortProcessor _processor;

        private void OnEnable()
        {
            _processor = new UndistortProcessor();
            VisionPipeline.AddProcessor(_processor);
        }

        private void OnDisable()
        {
            VisionPipeline.RemoveProcessor(_processor);
        }
    }
}
