﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MowingPlanetCompany
{
    public class PopUpController : MonoBehaviour
    {
        /// <summary> ポップアップの停止場所 </summary>
        [SerializeField] Transform m_anchor;
        /// <summary> ポップアップの時間 </summary>
        [SerializeField] float m_time = 0.5f;
        /// <summary> RGB値を扱う変数 </summary>
        float red, green, blue;
        /// <summary> A値(透明度)を扱う変数 </summary>
        float alfa;
        /// <summary> フェードインの時間 </summary>
        float speed = 0.01f;

        // Use this for initialization
        void Start()
        {
            if (this.gameObject.name == "Title")
            {
                red = GetComponent<TextMeshProUGUI>().color.r;
                green = GetComponent<TextMeshProUGUI>().color.g;
                blue = GetComponent<TextMeshProUGUI>().color.b;
            }
            else
            {
                red = GetComponent<Image>().color.r;
                green = GetComponent<Image>().color.g;
                blue = GetComponent<Image>().color.b;
            }
                iTween.MoveTo(this.gameObject, iTween.Hash("x", m_anchor.position.x, "y", m_anchor.position.y, "time", m_time));
        }

        private void Update()
        {
            if (this.gameObject.name == "Title")
            {
                GetComponent<TextMeshProUGUI>().color = new Color(red, green, blue, alfa);
            }
            else
            {
                GetComponent<Image>().color = new Color(red, green, blue, alfa);
            }
            alfa += speed;
        }
    }
}
