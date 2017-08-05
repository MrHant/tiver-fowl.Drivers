using System.Configuration;

namespace Tiver.Fowl.Drivers.Configuration
{
    public class DriverCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new DriverElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DriverElement) element).Name;
        }
    }
}
