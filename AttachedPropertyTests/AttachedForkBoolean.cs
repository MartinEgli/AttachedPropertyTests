// -----------------------------------------------------------------------
// <copyright file="AttachedForkBoolean.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    public abstract class AttachedForkBoolean<TOwner> : AttachedFork<bool, TOwner>
    {
    }

    public sealed class AttachedForkBoolean : AttachedForkBoolean<AttachedForkBoolean>
    {
    }
}