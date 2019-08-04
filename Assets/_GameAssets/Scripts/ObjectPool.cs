using System.Collections.Generic;
using UnityEngine;



namespace SpeedyBoat
{
    public class ObjectPool
    {
        public readonly List<GameObject> ActiveObjects = new List<GameObject>();



        public void Destroy()
        {
            Reset();

            foreach(var obj in m_poolObjects)
            {
                Object.Destroy(obj);
            }

            m_poolObjects.Clear();

            Object.Destroy(m_parent.gameObject);
        }



        public void Reset()
        {
            if (ActiveObjects.Count > 0)
            {
                var objectsCopy = new List<GameObject>(ActiveObjects);
                foreach (var activeObj in objectsCopy)
                {
                    FreeObject(activeObj);
                }
            }
        }



        public ObjectPool(string name, GameObject prefab, Transform parent, int count)
        {
            m_name = name;

            m_prefab = prefab;
            m_prefab.SetActive(false);

            m_parent = parent;

            for (int i = 0; i < count; ++i)
            {
                var go = Object.Instantiate(prefab);
                go.name = prefab.name + i;
                go.transform.SetParent(parent, false);

                m_poolObjects.Add(go);
            }
        }



        public GameObject AllocateObject()
        {
            if (m_poolObjects.Count > 0)
            {
                var go = m_poolObjects[0];
                //Debug.Log(m_name + "Allocates " + go.name);

                m_poolObjects.RemoveAt(0);
                ActiveObjects.Add(go);

                return go;
            }

            return null;
        }



        public void FreeObject(GameObject go)
        {
            Debug.Assert(ActiveObjects.Count > 0, "ObjectPool " + m_name + " Freeing object " + go.name + " but has no Active Objects!");

            if (ActiveObjects.Count > 0)
            {
                Debug.Assert(ActiveObjects.Contains(go), "ObjectPool " + m_name + " Freeing object " + go.name + " but it's not in the Active list!");
            }

            //Debug.Log(m_name + "Frees " + go.name);

            go.transform.SetParent(m_parent);

            go.SetActive(false);

            ActiveObjects.Remove(go);
            m_poolObjects.Insert(0, go);
        }



        private readonly string             m_name;
        private readonly GameObject         m_prefab;
        private readonly Transform          m_parent;
        private readonly List<GameObject>   m_poolObjects = new List<GameObject>();
    }
}


