using UnityEngine;

public class RimLight : MonoBehaviour
{

        public Transform target;
        public float distance = 3f;
        public float height = 2f;
        public float sideOffset = 0.5f;

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 behind = -target.forward * distance;
            Vector3 side = target.right * sideOffset;
            Vector3 up = Vector3.up * height;

            transform.position = target.position + behind + side + up;
            transform.rotation = Quaternion.LookRotation(target.forward);
        }
    
}
