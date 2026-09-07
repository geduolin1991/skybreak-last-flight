using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {
// Runtime-created GPU resources must follow their scenery or effect owner.
public sealed class SkyOwnedResources : MonoBehaviour {
 readonly List<Object> owned=new List<Object>();
 public T Keep<T>(T resource) where T:Object {owned.Add(resource);return resource;}
 void OnDestroy(){foreach(var resource in owned)if(resource)Destroy(resource);owned.Clear();}
}
}
