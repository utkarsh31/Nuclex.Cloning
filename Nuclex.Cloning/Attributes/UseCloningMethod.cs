using System;

namespace Nuclex.Cloning.Attributes
{
    public class UseCloningMethod:Attribute
    {
        public UseCloningMethod(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}