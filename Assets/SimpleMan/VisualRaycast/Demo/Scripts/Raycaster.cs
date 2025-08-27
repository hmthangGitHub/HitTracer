using UnityEngine;

namespace SimpleMan.VisualRaycast.Demo
{
    public class Raycaster : MonoBehaviour
    {
        //******     FIELDS AND PROPERTIES   	******\\
        public enum CastType
        {
            Raycast,
            Boxcast,
            Spherecast
        }
        public CastType castType = CastType.Raycast;
        public bool castAll;




        //******    	    METHODS  	  	    ******\\
        private void Update()
        {
            CastResult castResult;

            //Make cast from origin position to forward
            switch (castType)
            {
                case CastType.Raycast:
                    castResult = this.Raycast(castAll, transform.position, transform.forward); break;

                case CastType.Boxcast:
                    castResult = this.Boxcast(castAll, transform.position, transform.forward, Vector3.one); break;

                case CastType.Spherecast:
                    castResult = this.SphereCast(castAll, transform.position, transform.forward, 0.5f); break;
            }

            //Did raycast hit something? -> Try paint it
            if (castResult)
                PaintCastTargets(castResult, Color.white);


            if (Input.GetKeyDown(KeyCode.S))
                MakeSphereOverlap();

            else if (Input.GetKeyDown(KeyCode.B))
                MakeBoxOverlap();
        }

        /// <summary>
        /// Change color of material on target game object
        /// </summary>
        /// <param name="target"> Target game object </param>
        /// <param name="newColor"> New color </param>
        private void PaintCastTargets(CastResult result, Color newColor)
        {
            foreach (var item in result.Hits)
            {
                if (item.transform.TryGetComponent(out Renderer renderer))
                    renderer.material.color = newColor;
            }
        }

        private void MakeBoxOverlap()
        {
            this.BoxOverlap(transform.position, Vector3.one * 10);
        }

        private void MakeSphereOverlap()
        {
            this.SphereOverlap(transform.position, 5);
        }
    }
}