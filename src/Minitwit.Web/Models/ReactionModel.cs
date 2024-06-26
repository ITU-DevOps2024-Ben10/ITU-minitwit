﻿using Minitwit.Core.Entities;

namespace Minitwit.Web.Models;

public record ReactionModel
{
    public ReactionType ReactionType;
    public int ReactionCount;

    public ReactionModel(ReactionType reactionType, int reactionCount)
    {
        ReactionType = reactionType;
        ReactionCount = reactionCount;
    }
}
