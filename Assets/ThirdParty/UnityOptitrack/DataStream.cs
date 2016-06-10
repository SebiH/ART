/**
 * Adapted from johny3212
 * Written by Matt Oskamp
 */

namespace OptitrackManagement
{

    public class DataStream
    {

        public OptiTrackRigidBody[] _rigidBody = new OptiTrackRigidBody[200];
        public int _nRigidBodies = 0;

        public DataStream()
        {
            InitializeRigidBody();
        }

        public bool InitializeRigidBody()
        {
            _nRigidBodies = 0;
            for (int i = 0; i < 200; i++)
            {
                _rigidBody[i] = new OptiTrackRigidBody();
            }
            return true;
        }

        public OptiTrackRigidBody getRigidbody(int index)
        {
            return _rigidBody[index];
        }
    }
}