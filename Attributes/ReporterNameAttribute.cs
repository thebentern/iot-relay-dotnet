using System;

namespace IotRelay.Service
{
    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class ReporterNameAttribute : Attribute
    {
        readonly string name;

        public ReporterNameAttribute(string name) => this.name = name;

        public string Name
        {
            get { return name; }
        }
    }
}