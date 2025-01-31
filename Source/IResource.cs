﻿namespace ChainFx
{
    /// <summary>
    /// A provider that write its contents. 
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// Write out contents.
        /// </summary>
        void Write<C>(C cnt) where C : ContentBuilder, ISink;

        /// <summary>
        /// Converts contents into an appropriate content object.
        /// </summary>
        IContent Dump();
    }
}