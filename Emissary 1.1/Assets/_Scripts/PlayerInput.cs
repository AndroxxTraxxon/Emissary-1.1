using UnityEngine;
using System.Collections;

namespace Assets._Scripts {
    public class PlayerInput : MonoBehaviour {

        public bool enableDebug = false;

        private RaycastHit hit;
        bool didTheRaycastHit = false;

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            didTheRaycastHit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);

            if (Input.GetMouseButtonDown(0))
            {
                if (enableDebug)
                {

                    if (didTheRaycastHit == true && Field.instance != null)
                    {
                        StartCoroutine(Field.instance.OrientGrid(Field.instance.defaultGrid, hit.point));
                        //Debug.Log("The Clicky Stuff Happened!");
                    }
                    return;
                }
            }

        }

        void OnDrawGizmos()
        {
            if (Field.instance != null && didTheRaycastHit)
            {
                Vector3 center;
                Gizmos.color = Color.red;
                //Gizmos.DrawCube(hit.point, Vector3.one * Field.instance.nodeRadius / 4f);
                foreach (Node n in Field.instance.defaultGrid.getNodeQuad(hit.point, out center))
                {
                    Gizmos.DrawCube(n.globalPosition, Vector3.one * (Field.instance.nodeRadius + .25f)/2);
                }

                Gizmos.DrawCube(center, Vector3.one * (Field.instance.nodeRadius + .5f) / 2);

            }
        }
    }
}
