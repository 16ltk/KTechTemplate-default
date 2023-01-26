using System;

namespace KTechTemplateDAL
{

    //Use to manipulate PDF Create

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class HTMLMaskAttribute : Attribute
    {
        public HTMLMaskAttribute() { }
        public HTMLMaskAttribute(string name, bool value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public bool Value { get; private set; }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class ShowRequiredAttribute : Attribute
    {
        public ShowRequiredAttribute() { }
        public ShowRequiredAttribute(string name, bool value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public bool Value { get; private set; }
    }
}
