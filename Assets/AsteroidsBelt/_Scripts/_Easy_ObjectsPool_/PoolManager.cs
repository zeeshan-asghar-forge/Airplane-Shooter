using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolingObjects
    {
        public string customName;           // Optional custom name
        public GameObject objectPrefab;     // Prefab to pool
        public int quantity = 1;            // Number of instances
        public bool addAutoPoolScript = true;
    }

    public PoolingObjects[] preloadObjects;
    [HideInInspector] public List<GameObject> Pool = new List<GameObject>();

    [Header("Auto-Return Settings")]
    public float defaultLifetime = 3f; // Auto-return bullets after N seconds

    void Awake()
    {
        GameObject newObj;

        for (int i = 0; i < preloadObjects.Length; i++)
        {
            if (preloadObjects[i].customName == "")
                preloadObjects[i].customName = preloadObjects[i].objectPrefab.name;

            if (preloadObjects[i] != null)
            {
                for (int j = 0; j < preloadObjects[i].quantity; j++)
                {
                    newObj = Instantiate(preloadObjects[i].objectPrefab);
                    newObj.name = preloadObjects[i].customName;

                    if (preloadObjects[i].addAutoPoolScript)
                    {
                        if (!newObj.GetComponent<PooledObject>())
                            newObj.AddComponent<PooledObject>();
                        newObj.GetComponent<PooledObject>().parentPool = this;
                    }

                    PoolObject(newObj, false);
                }
            }
        }
    }

    public GameObject GetObjectByName(string objectName)
    {
        for (int i = 0; i < Pool.Count; i++)
        {
            if (Pool[i] == null) continue;
            if (Pool[i].name == objectName)
            {
                GameObject obj = Pool[i];
                Pool.RemoveAt(i);
                obj.transform.parent = null;
                obj.SetActive(true);
                return obj;
            }
        }
        return null;
    }

    public GameObject GetObjectByID(int id)
    {
        if (Pool.Count > id && Pool[id] != null)
        {
            GameObject obj = Pool[id];
            Pool.RemoveAt(id);
            obj.transform.parent = null;
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public GameObject GetRandomObject()
    {
        if (Pool.Count == 0) return null;

        int id = Mathf.FloorToInt(Random.Range(0, Pool.Count));
        if (Pool[id] == null) return null;

        GameObject obj = Pool[id];
        Pool.RemoveAt(id);
        obj.transform.parent = null;
        obj.SetActive(true);
        return obj;
    }

    public void PoolObject(GameObject _object, bool preloadedOnly)
    {
        if (!_object) return;

        if (!preloadedOnly)
        {
            ResetObjectTransform(_object);
            _object.SetActive(false);
            _object.transform.parent = transform;
            Pool.Add(_object);
        }
        else
        {
            foreach (var p in preloadObjects)
            {
                if (p.customName == _object.name)
                {
                    ResetObjectTransform(_object);
                    _object.SetActive(false);
                    _object.transform.parent = transform;
                    Pool.Add(_object);
                    break;
                }
            }
        }
    }

    public GameObject ResetObjectTransform(GameObject _object)
    {
        if (!_object) return null;

        _object.transform.parent = transform;
        _object.transform.localPosition = Vector3.zero;
        _object.transform.localRotation = Quaternion.identity;

        Rigidbody rb = _object.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        return _object;
    }

    public void AutoReturn(GameObject _object, float lifetime = -1f)
    {
        if (_object == null) return;
        if (lifetime <= 0f) lifetime = defaultLifetime;
        StartCoroutine(AutoReturnRoutine(_object, lifetime));
    }

    private IEnumerator AutoReturnRoutine(GameObject obj, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (obj != null) PoolObject(obj, false);
    }
}
