namespace Microsoft.Hpc
{
    using System;

    [Obsolete("Please consider refactor the logic here. It might be a bug")]
    public class Refactor : Attribute
    {
        public Refactor(string message)
        {

        }
    }
}
