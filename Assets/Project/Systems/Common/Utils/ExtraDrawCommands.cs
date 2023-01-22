using Drawing;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RR.Utils
{
    public static class ExtraDrawCommands
    {
        private static (Mesh mesh, bool loaded, bool loading) _icoSphere;
        private static (Mesh mesh, bool loaded, bool loading) _cube;
        
        public static void DrawPoint(this CommandBuilder commandBuilder, Vector3 position, float radius)
        {
            var u1 = position + Vector3.up * (radius);
            var u2 = position - Vector3.up * (radius);
            var r1 = position + Vector3.right * (radius);
            var r2 = position - Vector3.right * (radius);
            var f1 = position + Vector3.forward * (radius);
            var f2 = position - Vector3.forward * (radius);
            
            commandBuilder.Line(u1, u2);
            commandBuilder.Line(r1, r2);
            commandBuilder.Line(f1, f2);
        }

        public static void DrawSolidSphere(this CommandBuilder commandBuilder, Vector3 position, Vector3 scale)
        {
            LoadIcoSphere(false);
            if(!_icoSphere.loaded)
                return;
            Matrix4x4 trs = Matrix4x4.TRS(position, Quaternion.identity, scale);
            using (commandBuilder.WithMatrix(trs))
            {
                commandBuilder.SolidMesh(_icoSphere.mesh);
            }
        }

        public static void DrawSolidCube(this CommandBuilder commandBuilder, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            LoadCube(false);
            if(!_cube.loaded)
                return;
            Matrix4x4 trs = Matrix4x4.TRS(position, rotation, scale);
            using (commandBuilder.WithMatrix(trs))
            {
                commandBuilder.SolidMesh(_cube.mesh);
            }
        }
        
        public static void DrawSolidLine(this CommandBuilder commandBuilder, Vector3 from, Vector3 to, float radius)
        {
            LoadCube(false);
            if(!_cube.loaded)
                return;
            var scale = new Vector3	(radius, radius, radius + (to - from).magnitude * 0.5f);
            var toFrom = to - from;
            if(to == from)
                toFrom = Vector3.forward;
            var trs = Matrix4x4.TRS((from + to) * 0.5f, Quaternion.LookRotation(toFrom, Vector3.up), scale);
            using (commandBuilder.WithMatrix(trs))
            {
                commandBuilder.SolidMesh(_cube.mesh);
            }
        }

        #region Load Mesh

        private static void LoadIcoSphere(bool forceLoad)
        {
            if ((_icoSphere.loaded && !forceLoad) || _icoSphere.loading)
                return;
            var handle = Addressables.LoadAssetAsync<Mesh>("Icosphere.mesh");
            _icoSphere = (null, false, true);
            handle.Completed += delegate(AsyncOperationHandle<Mesh> operationHandle)
            {
                if(operationHandle.Status == AsyncOperationStatus.Failed)
                    return;
                _icoSphere = (operationHandle.Result, true, false);
                Debug.Log("Loaded IcoSphere");
            };
        }
        private static void LoadCube(bool forceLoad)
        {
            if ((_cube.loaded && !forceLoad) || _cube.loading)
                return;
            var handle = Addressables.LoadAssetAsync<Mesh>("Cube.mesh");
            _cube = (null, false, true);
            handle.Completed += delegate(AsyncOperationHandle<Mesh> operationHandle)
            {
                if(operationHandle.Status == AsyncOperationStatus.Failed)
                    return;
                _cube = (operationHandle.Result, true, false);
                Debug.Log("Loaded Cube");
            };
        }

        #endregion
    }
}