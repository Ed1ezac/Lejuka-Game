using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lejuka.Unity.Editor
{
    /// <summary>
    /// Finds scene objects by name, inactive ones included, and reports the ones that are not there.
    /// The search is by name rather than by exact path, so an extra layer of grouping in the scene does not break it.
    /// </summary>
    public sealed class SceneObjectFinder
    {
        private readonly Scene _scene;
        private readonly WiringReport _report;

        /// <summary>Creates a finder for one scene.</summary>
        public SceneObjectFinder(Scene scene, WiringReport report)
        {
            _scene = scene;
            _report = report;
        }

        /// <summary>Finds an object anywhere in the scene. Reports it and returns <c>null</c> when it is missing.</summary>
        public GameObject Find(string objectName)
        {
            foreach (var root in _scene.GetRootGameObjects())
            {
                var match = FindDeep(root.transform, objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            _report.Problem($"Scene object '{objectName}' not found.");
            return null;
        }

        /// <summary>
        /// Finds an object below <paramref name="parent"/>. Reports it and returns <c>null</c> when it is missing.
        /// A missing parent has been reported already, so its children are named but not searched for.
        /// </summary>
        public GameObject FindUnder(GameObject parent, string parentName, string objectName)
        {
            if (parent == null)
            {
                _report.Problem($"Scene object '{parentName}/{objectName}' not found (its parent '{parentName}' is missing).");
                return null;
            }

            foreach (Transform child in parent.transform)
            {
                var match = FindDeep(child, objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            _report.Problem($"Scene object '{parentName}/{objectName}' not found.");
            return null;
        }

        /// <summary>Returns the component on an object. Reports it and returns <c>null</c> when it is missing.</summary>
        public T Require<T>(GameObject target)
            where T : Component
        {
            if (target == null)
            {
                return null;
            }

            if (target.TryGetComponent<T>(out var component))
            {
                return component;
            }

            _report.Problem($"'{target.name}' has no {typeof(T).Name} component.");
            return null;
        }

        // Breadth-first, so when a name is used twice the one nearest the top wins.
        private static Transform FindDeep(Transform start, string objectName)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.name == objectName)
                {
                    return current;
                }

                foreach (Transform child in current)
                {
                    queue.Enqueue(child);
                }
            }

            return null;
        }
    }
}
