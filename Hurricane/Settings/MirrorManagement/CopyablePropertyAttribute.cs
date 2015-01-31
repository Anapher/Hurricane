using System;

namespace Hurricane.Settings.MirrorManagement
{
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    internal sealed class CopyablePropertyAttribute : Attribute
    {
        public bool CopyContainingProperties { get; set; }
    }
}
