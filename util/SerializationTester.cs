using UnityEngine;

namespace com.jsch.UnityUtil
{
    public class SerializationTester : MonoBehaviour
    {
        public ScriptableObject ScriptableObject
        {
            get
            {
                if (_scriptableObject == null && !string.IsNullOrEmpty(serializedJSON))
                    _scriptableObject = Serializer.Deserialize<ScriptableObject>(serializedJSON);
                return _scriptableObject;
            }
            set
            {
                serializedJSON = Serializer.Serialize(value);
                _scriptableObject = value;
            }
        }

        [SerializeField] private string serializedJSON;
        private ScriptableObject _scriptableObject;

        private void OnValidate()
        {
            _scriptableObject = !string.IsNullOrEmpty(serializedJSON) ?
                Serializer.Deserialize<ScriptableObject>(serializedJSON) : null;
        }
    }
}