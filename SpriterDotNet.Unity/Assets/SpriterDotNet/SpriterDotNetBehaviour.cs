// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace SpriterDotNetUnity
{
    [Serializable]
    public class ChildData
    {
        public GameObject[] SpritePivots;
        public GameObject[] Sprites;
        public GameObject[] BoxPivots;
        public GameObject[] Boxes;
        public GameObject[] Points;

        public Transform[] SpritePivotTransforms;
        public Transform[] SpriteTransforms;
        public Transform[] BoxPivotTransforms;
        public Transform[] BoxTransforms;
        public Transform[] PointTransforms;
    }

    [ExecuteInEditMode]
    public class SpriterDotNetBehaviour : MonoBehaviour
    {
        [HideInInspector]
        public string SortingLayer;

        [HideInInspector]
        public int SortingOrder;

        public float Ppu = 100;

        [HideInInspector]
        public ChildData ChildData;

        [HideInInspector]
        public int EntityIndex;

        //        [HideInInspector]
        public SpriterData SpriterData;

        [HideInInspector]
        public bool UseNativeTags;

        [HideInInspector]
        public bool UseUi;

        public SpriterDotNet.Animator<Sprite, AudioClip> Animator { get; private set; }

        private string defaultTag;

        Queue<string> animationQueue = new Queue<string>();

        void Awake()
        {
            EnsureAnimator();
        }

        void EnsureAnimator()
        {
            if (SpriterData == null)
            {
                Debug.LogWarning("SpriterData not found for " + name);
            }
            else if (SpriterData.Spriter.Entities.Length <= EntityIndex)
            {
                Debug.LogError("Entity not found in SpriterData with index " + EntityIndex + " for " + name);
            }
            else if (Animator == null)
            {
                SpriterEntity entity = SpriterData.Spriter.Entities[EntityIndex];
                AudioSource audioSource = gameObject.GetComponent<AudioSource>();

                if (!UseUi)
                {
                    Animator = new UnityAnimator(entity, ChildData, audioSource, Ppu);
                }
                else
                {
                    Animator = new UnityUiAnimator(entity, ChildData, audioSource, Ppu);
                }
                RegisterSpritesAndSounds();

                if (UseNativeTags)
                    defaultTag = gameObject.tag;

                Animator.AnimationFinished += (animName) => CheckForQueue();
            }
        }

        public void Start()
        {
            EnsureAnimator();
            Animator.Update(0);
        }

        void CheckForQueue()
        {
            if (animationQueue.Count > 0)
            {
                var next = animationQueue.Dequeue();
                Animator.Speed = 1.0f;
                Animator.Play(next);
            }
        }

        public void Queue(string animationName)
        {
            animationQueue.Enqueue(animationName);
        }

        public void ClearQueue()
        {
            animationQueue.Clear();
        }

        public void Update()
        {
            Tick(false);
        }

        public void Tick(bool playAnimationInEditor)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !playAnimationInEditor)
                return;
#endif

            if (Animator == null)
                return;

            if (Animator is UnityAnimator)
            {
                (Animator as UnityAnimator).SortingLayer = SortingLayer;
                (Animator as UnityAnimator).SortingOrder = SortingOrder;
            }
            Animator.Update(Time.deltaTime * 1000.0f);

            if (UseNativeTags)
            {
                var tags = Animator.FrameData.AnimationTags;
                if (tags != null && tags.Count > 0)
                    gameObject.tag = tags[0];
                else
                    gameObject.tag = defaultTag;
            }
        }

        private void RegisterSpritesAndSounds()
        {
            foreach (SdnFileEntry entry in SpriterData.FileEntries)
            {
                if (entry.Sprite != null)
                    Animator.SpriteProvider.Set(entry.FolderId, entry.FileId, entry.Sprite);
                else
                    Animator.SoundProvider.Set(entry.FolderId, entry.FileId, entry.Sound);
            }
        }
    }
}
