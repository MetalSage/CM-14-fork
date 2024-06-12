﻿using System.Numerics;
using Content.Shared._CM14.Xenos.Animations;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using static Robust.Client.Animations.AnimationTrackProperty;

namespace Content.Client._CM14.Xenos.Animations;

public sealed class XenoAnimationsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    private const string MeleeLungeKey = "melee-lunge";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PlayLungeAnimationEvent>(OnPlayLungeAnimation);
    }

    private void OnPlayLungeAnimation(PlayLungeAnimationEvent ev)
    {
        var entityUid = GetEntity(ev.EntityUid);
        var direction = ev.Direction;

        var animation = GetLungeAnimation(direction);
        _animation.Stop(entityUid, MeleeLungeKey);
        _animation.Play(entityUid, animation, MeleeLungeKey);
    }

    private Animation GetLungeAnimation(Vector2 direction)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(0.4f),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new KeyFrame(Vector2.Zero, 0f),
                        new KeyFrame(direction.Normalized() * 0.6f, 0.1f),
                        new KeyFrame(Vector2.Zero, 0.3f)
                    }
                }
            }
        };
    }
}