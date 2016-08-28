namespace Assets.Modules.Vision.CameraSources
{
    class EmptyCameraSource : CameraSource
    {
        public override void InitCamera()
        {
            ImageProcessing.SetEmptyCamera();
        }
    }
}
