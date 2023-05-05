using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectCollections.Runtime
{
    public class SOCollection<T> : ScriptableObject where T : ScriptableObject
    {
        public bool AutoAddFromFolder = true;
        public bool AutoRemove = true;
        public List<T> DataObjects = new List<T>();
    }
}
