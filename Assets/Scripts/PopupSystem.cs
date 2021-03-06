﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MowingPlanetCompany
{
    /// <summary>
    /// popupさせたいオブジェクトの呼び出し側にこのscriptをアタッチ
    /// inspectorにpopupさせたいオブジェクト(UI)とその親となるcanvas,
    /// </summary>
    public class PopupSystem : MonoBehaviour
    {
        Canvas canvas;
        GameObject canvasObject;
        GameObject popupedObject;

        [SerializeField] GameObject popupObject;
        [SerializeField] float scalingTime = 1f;


        /// <summary>
        /// UIに配置されているボタンに対してイベントハンドラを登録する
        /// </summary>
        /// <param name="popupMaterial"></param>
        public void SubscribeButton(PopupSystemMaterial popupSystemMaterial)
        {
            var buttons = popupedObject.GetComponentsInChildren<Button>().ToList();
            var button = buttons.Find(obj => obj.gameObject.name == popupSystemMaterial.ButtonName);

            button.onClick.AddListener(() =>
            {
                popupSystemMaterial.EventHandler();
                if (popupSystemMaterial.IsPushAfterClose)
                {
                    Close();
                }
            });
        }

        /// <summary>
        /// Scene上にCanvasを作成
        /// </summary>
        void CreateCanvas()
        {
            // Create canvas
            canvasObject = new GameObject("PopupCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 最前面に来る様適当なSortingOderに設定する
            canvas.sortingOrder = 100;
        }

        /// <summary>
        /// 引数のUIをポップアップさせる
        /// Popupさせるポジションは予めCanvasに配置してTransform情報を保持したPrefabから取得する
        /// </summary>
        public void Popup()
        {
            if (canvas == null)
            {
                CreateCanvas();
            }
            popupedObject = Instantiate(popupObject, canvas.transform);
            popupedObject.transform.localScale = new Vector3(1f, 0f, 1f);
            iTween.ScaleTo(popupedObject, iTween.Hash("scale", Vector3.one, "time", scalingTime));
        }

        /// <summary>
        /// popupしたオブジェクトを閉じた後削除
        /// </summary>
        public void Close()
        {
#if true
            iTween.ScaleTo(popupObject, iTween.Hash("scale", new Vector3(1f, 0f, 0f), "time", scalingTime));
            Destroy(canvasObject, scalingTime);
#else
            Destroy(canvasObject);
#endif
        }
    }
}