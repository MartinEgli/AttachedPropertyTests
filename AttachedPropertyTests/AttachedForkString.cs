﻿// -----------------------------------------------------------------------
// <copyright file="AttachedForkString.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AttachedPropertyTests
{
    public sealed class AttachedForkString : AttachedForkString<AttachedForkString>
    {
    }

    public abstract class AttachedForkString<TOwner> : AttachedFork<string, TOwner>
    {
    }
}