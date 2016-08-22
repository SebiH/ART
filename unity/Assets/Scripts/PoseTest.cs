using UnityEngine;
using System.Collections;
using Assets.Scripts.RealCamera;

public class PoseTest : MonoBehaviour
{

    void Start()
    {
    }

    void Update()
    {
        if (ImageProcessing.HasNewPose())
        {
            var transformMatrix = new Matrix4x4();
            transformMatrix.m33 = 1;

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    transformMatrix[row, col] = (float)ImageProcessing.GetPose(row * 4 + col);
                }
            }

            var pos = ExtractTranslationFromMatrix(ref transformMatrix);
            // ARToolkit cm -> Unity m
            pos = pos / 100f;
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
