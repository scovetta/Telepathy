// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.QueueAdapter.DTO
{
    using System;

    public class ParameterUnpacker
    {
        private object[] parameters;

        private int idx;

        public ParameterUnpacker(object[] parameters)
        {
            this.parameters = parameters;
            this.idx = 0;
        }

        public ParameterUnpacker Unpack<T>(out T strongTypedParameter)
        {
            if (this.idx <= this.parameters.Length)
            {
                var param = this.parameters[this.idx];

                if (param is T res)
                {
                    strongTypedParameter = res;
                    this.idx++;
                    return this;
                }
                else
                {
                    throw new InvalidOperationException($"Argument type mismatch. Expected {typeof(T)}, get {param.GetType()}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Trying to get more parameters than received");
            }
        }

        public ParameterUnpacker UnpackInt(out int strongTypedParameter)
        {
            this.Unpack<long>(out var l);
            strongTypedParameter = (int)l;
            return this;
        }

        public ParameterUnpacker UnpackString(out string strongTypedParameter)
        {
            this.Unpack<string>(out strongTypedParameter);
            return this;
        }
    }
}