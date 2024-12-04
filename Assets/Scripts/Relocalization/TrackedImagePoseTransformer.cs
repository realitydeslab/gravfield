// SPDX-FileCopyrightText: Copyright 2023 Reality Design Lab <dev@reality.design>
// SPDX-FileContributor: Yuchen Zhang <yuchenz27@outlook.com>
// SPDX-FileContributor: Botao Amber Hu <botao.a.hu@gmail.com>
// SPDX-License-Identifier: MIT

using UnityEngine;
#if UNITY_IOS
using HoloKit.iOS;
#endif

namespace HoloKit.ColocatedMultiplayerBoilerplate
{
    public class TrackedImagePoseTransformer : MonoBehaviour
    {
        public Vector3 worldOriginPosition;
#if UNITY_IOS
        [SerializeField] private WorldOriginResetter m_WorldOriginResetter;
#endif
        public void OnTrackedImageStablized(Vector3 position, Quaternion rotation)
        {
            rotation = rotation * Quaternion.Euler(90f, 0f, 0f);

            var r = Matrix4x4.Rotate(rotation);
            var a = r.m00 + r.m22;
            var b = -r.m20 + r.m02;
            float thetaInDeg = Mathf.Atan2(b, a) / Mathf.Deg2Rad;
#if UNITY_IOS
            m_WorldOriginResetter.ResetWorldOrigin(position - worldOriginPosition, Quaternion.AngleAxis(thetaInDeg, Vector3.up));
#endif
        }
    }
}
