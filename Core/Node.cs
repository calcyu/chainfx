﻿using System;

namespace Greatbone.Core
{
    /// <summary>
    /// A resolved node of the controller hierarchy corresponding to a segment in URI path.
    /// </summary>
    public struct Node
    {
        readonly string key;

        readonly string label;

        readonly Work work;

        internal Node(string key, string label, Work work)
        {
            this.key = key;
            this.label = label;
            this.work = work;
        }

        public string Key => key;

        public string Label => label;

        public Type Type => work?.GetType();

        public Work Work => work;

        //
        // CONVERSION
        //

        public static implicit operator bool(Node v)
        {
            string str = v.key;
            if (!string.IsNullOrEmpty(str))
            {
                return "true".Equals(str) || "1".Equals(str) || "on".Equals(str);
            }
            return false;
        }

        public static implicit operator short(Node v)
        {
            string str = v.key;
            if (!string.IsNullOrEmpty(str))
            {
                if (short.TryParse(str, out var n))
                {
                    return n;
                }
            }
            return 0;
        }

        public static implicit operator int(Node v)
        {
            string str = v.key;
            if (!string.IsNullOrEmpty(str))
            {
                if (int.TryParse(str, out var n))
                {
                    return n;
                }
            }
            return 0;
        }

        public static implicit operator long(Node v)
        {
            string str = v.key;
            if (!string.IsNullOrEmpty(str))
            {
                if (long.TryParse(str, out var n))
                {
                    return n;
                }
            }
            return 0;
        }

        public static implicit operator decimal(Node v)
        {
            string str = v.key;
            if (!string.IsNullOrEmpty(str))
            {
                if (decimal.TryParse(str, out var n))
                {
                    return n;
                }
            }
            return 0;
        }

        public static implicit operator DateTime(Node v)
        {
            return default;
        }

        public static implicit operator string(Node v)
        {
            return v.key;
        }
    }
}