using UnityEngine;

namespace Projectiles
{
    [DefaultExecutionOrder(-100)]
    public class Scene : MonoBehaviour
    {
        // PUBLIC MEMBERS

        public SceneContext Context => _context;

        // PRIVATE MEMBERS

        [SerializeField]
        private SceneContext _context;

        // MONOBEHAVIOUR

        protected void Update()
        {
            // Validate network related objects before non-network services will try to access it
            ValidateContext();
        }

        // PRIVATE METHODS

        private void ValidateContext()
        {
            var runner = Context.Runner;
            //Debug.Log("Scene ValidateContext>>" + runner.transform.name);

            if (runner == null || runner.IsRunning == false)
            {
                Context.LocalAgent = null;
                return;
            }
            //Debug.Log("ValidateContext>>" + runner.LocalPlayer);
            //var localPlayer = Context.Runner.GetPlayerObject(runner.LocalPlayer);
            //Debug.Log("Scene ValidateContext localPlayer" + localPlayer.transform.name);
            //Context.LocalAgent = localPlayer != null ? localPlayer.GetComponent<Player>().ActiveAgent : null;
        }
    }
}
