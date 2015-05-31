using System.Configuration;

namespace KoenZomers.Omnik.Api.Configuration
{
    /// <summary>
    /// Configuration section for the Listeners
    /// </summary>
    public class ListenerConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ListenerConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ListenerConfigurationElement)element).Name;
        }

        public ListenerConfigurationElement this[int index]
        {
            get
            {
                return (ListenerConfigurationElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        new public ListenerConfigurationElement this[string name]
        {
            get
            {
                return (ListenerConfigurationElement)BaseGet(name);
            }
        }

        public int IndexOf(ListenerConfigurationElement element)
        {
            return BaseIndexOf(element);
        }

        public void Add(ListenerConfigurationElement element)
        {
            BaseAdd(element);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(ListenerConfigurationElement element)
        {
            if (BaseIndexOf(element) >= 0)
                BaseRemove(element.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }
    }
}
