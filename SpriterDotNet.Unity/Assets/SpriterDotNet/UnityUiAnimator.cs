// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriterDotNetUnity
{
    public class UnityUiAnimator : Animator<Sprite, AudioClip>
    {
        public string SortingLayer { get; set; }

        public int SortingOrder { get; set; }

        private const float DefaultPivot = 0.5f;

        private ChildData childData;
        private Image[] renderers;
        private AudioSource audioSource;
        private float ppu;
        private int index;
        private int boxIndex;
        private int pointIndex;

        public UnityUiAnimator(SpriterEntity entity, ChildData childData, AudioSource audioSource, float ppu)
            : base(entity)
        {
            this.childData = childData;
            this.audioSource = audioSource;
            this.ppu = ppu;

            renderers = new Image[childData.Sprites.Length];
            for (int i = 0; i < childData.Sprites.Length; ++i)
            {
                renderers[i] = childData.Sprites[i].GetComponent<Image>();
            }
        }

        protected override void Animate(float deltaTime)
        {
            index = 0;
            boxIndex = 0;
            pointIndex = 0;

            base.Animate(deltaTime);

            while (index < childData.Sprites.Length)
            {
                renderers[index].sprite = null;
                childData.Sprites[index].SetActive(false);
                childData.SpritePivots[index].SetActive(false);
                ++index;
            }
            while (boxIndex < childData.Boxes.Length)
            {
                childData.Boxes[boxIndex].SetActive(false);
                childData.BoxPivots[boxIndex].SetActive(false);
                ++boxIndex;
            }
            while (pointIndex < childData.Points.Length)
            {
                childData.Points[pointIndex].SetActive(false);
                ++pointIndex;
            }
        }

        protected override void ApplySpriteTransform(Sprite sprite, SpriterObject info)
        {
            GameObject child = childData.Sprites[index];
            GameObject pivot = childData.SpritePivots[index];
            RectTransform childTransform = (RectTransform)childData.SpriteTransforms[index];
            RectTransform pivotTransform = (RectTransform)childData.SpritePivotTransforms[index];

            child.SetActive(true);
            pivot.SetActive(true);
            Image renderer = renderers[index];

            renderer.sprite = sprite;
            Vector3 size = sprite.bounds.size * renderer.sprite.pixelsPerUnit;

            Color c = renderer.color;
            renderer.color = new Color(c.r, c.g, c.b, info.Alpha);
            childTransform.sizeDelta = size;
            pivotTransform.localEulerAngles = new Vector3(0, 0, info.Angle);
            pivotTransform.anchoredPosition = new Vector3(info.X, info.Y, 0);
            childTransform.pivot = new Vector2(info.PivotX, info.PivotY);
            childTransform.localScale = new Vector3(info.ScaleX, info.ScaleY, 1);

            renderer.transform.SetSiblingIndex(SortingOrder * renderers.Length + index);

            ++index;
        }

        protected override void ApplyBoxTransform(SpriterObjectInfo objInfo, SpriterObject info)
        {
            GameObject child = childData.Boxes[boxIndex];
            GameObject pivot = childData.BoxPivots[boxIndex];
            Transform childTransform = childData.BoxTransforms[boxIndex];
            Transform pivotTransform = childData.BoxPivotTransforms[boxIndex];
            child.SetActive(true);
            pivot.SetActive(true);

            float w = objInfo.Width / ppu;
            float h = objInfo.Height / ppu;

            BoxCollider2D collider = child.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(w, h);

            child.name = objInfo.Name;

            float deltaX = (DefaultPivot - info.PivotX) * w * info.ScaleX;
            float deltaY = (DefaultPivot - info.PivotY) * h * info.ScaleY;

            pivotTransform.localEulerAngles = new Vector3(0, 0, info.Angle);
            pivotTransform.localPosition = new Vector3(info.X / ppu, info.Y / ppu, 0);
            childTransform.localPosition = new Vector3(deltaX, deltaY, childTransform.localPosition.z);
            childTransform.localScale = new Vector3(info.ScaleX, info.ScaleY, 1);
            ++boxIndex;
        }

        protected override void ApplyPointTransform(string name, SpriterObject info)
        {
            GameObject point = childData.Points[pointIndex];
            RectTransform pointTransform = (RectTransform)childData.PointTransforms[pointIndex];
            point.name = name;
            point.SetActive(true);

            float x = info.X;
            float y = info.Y;

            pointTransform.anchoredPosition = new Vector2(x, y);

            ++pointIndex;
        }

        protected override void PlaySound(AudioClip sound, SpriterSound info)
        {
            #if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            #endif

            audioSource.panStereo = info.Panning;
            audioSource.PlayOneShot(sound, info.Volume);
        }

        /// <summary>
        /// In order to compile on console with AOT-Only, we must provide public definitions for some of the generic classes used internally.
        /// </summary>
        public Dictionary<int, Sprite> aot_SpritesByInt = new Dictionary<int, Sprite>();
        public Dictionary<int, IDictionary<int, Sprite>> aot_SpriteByIntNested = new Dictionary<int, IDictionary<int, Sprite>>();
        public Dictionary<int, AudioClip> aot_AudioByInt = new Dictionary<int, AudioClip>();
        public Dictionary<int, IDictionary<int, AudioClip>> aot_AudioByIntNested = new Dictionary<int, IDictionary<int, AudioClip>>();
    }
}
