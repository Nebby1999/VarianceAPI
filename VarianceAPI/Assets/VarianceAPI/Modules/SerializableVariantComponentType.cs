using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Components;

namespace VarianceAPI
{
    [Serializable]
    public struct SerializableVariantComponentType
    {
        public SerializableVariantComponentType(string typeName)
        {
            this._typeName = "";
            this.typeName = typeName;
        }
        public SerializableVariantComponentType(Type componentType)
        {
            this._typeName = "";
            this.componentType = componentType;
        }

        public string typeName
        {
            get
            {
                return this._typeName;
            }
            private set
            {
                this.componentType = Type.GetType(value);
            }
        }

        public Type componentType
        {
            get
            {
                if (this._typeName == null)
                {
                    return null;
                }
                Type type = Type.GetType(this._typeName);
                if (!(type != null) || !type.IsSubclassOf(typeof(VariantComponent)))
                {
                    return null;
                }
                return type;
            }
            set
            {
                this._typeName = ((value != null && value.IsSubclassOf(typeof(VariantComponent))) ? value.AssemblyQualifiedName : "");
            }
        }
        [SerializeField]
        private string _typeName;
    }
}
