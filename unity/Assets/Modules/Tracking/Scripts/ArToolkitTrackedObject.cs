using Assets.Modules.Vision;
using Assets.Modules.Vision.Outputs;
using Assets.Modules.Vision.Processors;
using System;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ArToolkitPoseTracking : MonoBehaviour
    {
        [Serializable]
        private class PoseMatrix
        {
            public float m00, m01, m02, m03;
            public float m10, m11, m12, m13;
            public float m20, m21, m22, m23;
        }

        [Serializable]
        private class Corners
        {
            public double[] topleft;
            public double[] topright;
            public double[] bottomleft;
            public double[] bottomright;
        }

        [Serializable]
        private class MarkerInfo
        {
            public int id;
            public double[] pos;
            public Corners corners;
            public PoseMatrix transform_matrix;
        }

        [Serializable]
        private class ArToolkitOutput
        {
            public MarkerInfo[] markers_left;
            public MarkerInfo[] markers_right;
        }


        public Pipeline ActivePipeline;

        private ArToolkitProcessor _artkProcessor;
        private JsonOutput _artkOutput;

        private bool _hasNewOutput = false;
        private ArToolkitOutput _currentOutput;

        void Start()
        {
            _artkProcessor = new ArToolkitProcessor();
            ActivePipeline.AddProcessor(_artkProcessor);

            _artkOutput = new JsonOutput(OnPoseChanged);
            ActivePipeline.AddOutput(_artkOutput);
        }

        void OnDestroy()
        {
            ActivePipeline.RemoveProcessor(_artkProcessor);
            ActivePipeline.RemoveOutput(_artkOutput);
        }

        private void OnPoseChanged(string msg)
        {
            try
            {
                _currentOutput = JsonUtility.FromJson<ArToolkitOutput>(msg);
                _hasNewOutput = true;
            }
            catch
            {
                Debug.LogError("Could not deserialize msg: \n" + msg);
            }
        }

        void Update()
        {
            if (_hasNewOutput)
            {
                _hasNewOutput = false;

                if (_currentOutput.markers_left.Length > 0)
                {
                    var pose = _currentOutput.markers_left[0].transform_matrix;
                    var transformMatrix = new Matrix4x4();

                    transformMatrix.m00 = pose.m00;
                    transformMatrix.m01 = pose.m01;
                    transformMatrix.m02 = pose.m02;
                    transformMatrix.m03 = pose.m03;

                    transformMatrix.m10 = pose.m10;
                    transformMatrix.m11 = pose.m11;
                    transformMatrix.m12 = pose.m12;
                    transformMatrix.m13 = pose.m13;

                    transformMatrix.m20 = pose.m20;
                    transformMatrix.m21 = pose.m21;
                    transformMatrix.m22 = pose.m22;
                    transformMatrix.m23 = pose.m23;

                    transformMatrix.m30 = 0;
                    transformMatrix.m31 = 0;
                    transformMatrix.m32 = 0;
                    transformMatrix.m33 = 1;

                    var pos = ExtractTranslationFromMatrix(ref transformMatrix);
                    // invert to match camera
                    pos.y = -pos.y;

                    transform.position = pos;
                    transform.localScale = ExtractScaleFromMatrix(ref transformMatrix);

                    var eulerRot = ExtractRotationFromMatrix(ref transformMatrix).eulerAngles;
                    eulerRot.x = -eulerRot.x;
                    //eulerRot.y = -eulerRot.y;
                    eulerRot.z = -eulerRot.z;

                    transform.rotation = Quaternion.Euler(eulerRot);
                }
                else
                {
                    Debug.Log("No left marker");
                }
            }
        }



        /*
         *  Methods below taken from http://forum.unity3d.com/threads/how-to-assign-matrix4x4-to-transform.121966/
         */




        /// <summary>
        /// Extract translation from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Translation offset.
        /// </returns>
        public Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        /// <summary>
        /// Extract rotation quaternion from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Quaternion representation of rotation transform.
        /// </returns>
        public Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        /// <summary>
        /// Extract scale from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Scale vector.
        /// </returns>
        private Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 scale = new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude
                );
            if (Vector3.Cross(matrix.GetColumn(0), matrix.GetColumn(1)).normalized != (Vector3)matrix.GetColumn(2).normalized)
            {
                scale.x *= -1;
            }
            return scale;
        }


        /// <summary>
        /// Extract position, rotation and scale from TRS matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <param name="localPosition">Output position.</param>
        /// <param name="localRotation">Output rotation.</param>
        /// <param name="localScale">Output scale.</param>
        public void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = ExtractTranslationFromMatrix(ref matrix);
            localRotation = ExtractRotationFromMatrix(ref matrix);
            localScale = ExtractScaleFromMatrix(ref matrix);
        }

        /// <summary>
        /// Set transform component from TRS matrix.
        /// </summary>
        /// <param name="transform">Transform component.</param>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        public void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
        {
            transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
            transform.localRotation = ExtractRotationFromMatrix(ref matrix);
            transform.localScale = ExtractScaleFromMatrix(ref matrix);
        }


        // EXTRAS!

        /// <summary>
        /// Identity quaternion.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Quaternion.identity</c>.</para>
        /// </remarks>
        public readonly Quaternion IdentityQuaternion = Quaternion.identity;
        /// <summary>
        /// Identity matrix.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Matrix4x4.identity</c>.</para>
        /// </remarks>
        public readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;

        /// <summary>
        /// Get translation matrix.
        /// </summary>
        /// <param name="offset">Translation offset.</param>
        /// <returns>
        /// The translation transform matrix.
        /// </returns>
        public Matrix4x4 TranslationMatrix(Vector3 offset)
        {
            Matrix4x4 matrix = IdentityMatrix;
            matrix.m03 = offset.x;
            matrix.m13 = offset.y;
            matrix.m23 = offset.z;
            return matrix;
        }
    }
}
