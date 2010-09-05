using System.Collections.Generic;
using System.Reflection;

namespace TinyTemplates
{
    class TemplateMemberAccessor
    {
        readonly string _memberName;

        public TemplateMemberAccessor(string memberName)
        {
            _memberName = memberName;
        }

        public object GetMember(Stack<object> model)
        {
            var m = model.Peek();
            var mi = m.GetType().GetProperty(_memberName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            return mi.GetValue(m, null);
        }
    }
}