﻿//------------------------------------------------------------
// Game Framework v2.x
// Copyright © 2014-2016 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using GameFramework.UI;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 界面组件。
    /// </summary>
    [AddComponentMenu("Game Framework/UI")]
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        private IUIManager m_UIManager = null;
        private EventComponent m_EventComponent = null;

        [SerializeField]
        private int m_InstanceCapacity = 8;

        [SerializeField]
        private Transform m_InstanceRoot = null;

        [SerializeField]
        private UIGroupHelperBase m_UIGroupHelperTemplate = null;

        [SerializeField]
        private UIFormHelperBase m_UIFormHelper = null;

        [SerializeField]
        private UIGroup[] m_UIGroups = null;

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount
        {
            get
            {
                return m_UIManager.UIGroupCount;
            }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的容量。
        /// </summary>
        public int InstanceCapacity
        {
            get
            {
                return m_UIManager.InstanceCapacity;
            }
            set
            {
                m_UIManager.InstanceCapacity = m_InstanceCapacity = value;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected internal override void Awake()
        {
            base.Awake();

            m_UIManager = GameFrameworkEntry.GetModule<IUIManager>();
            if (m_UIManager == null)
            {
                Log.Fatal("UI manager is invalid.");
                return;
            }

            m_UIManager.OpenUIFormSuccess += OnOpenUIFormSuccess;
            m_UIManager.OpenUIFormFailure += OnOpenUIFormFailure;
            m_UIManager.CloseUIFormComplete += OnCloseUIFormComplete;
        }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_EventComponent = GameEntry.GetComponent<EventComponent>();
            if (m_EventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            if (baseComponent.EditorResourceMode)
            {
                m_UIManager.SetResourceManager(baseComponent.EditorResourceHelper);
            }
            else
            {
                m_UIManager.SetResourceManager(GameFrameworkEntry.GetModule<IResourceManager>());
            }

            m_UIManager.SetObjectPoolManager(GameFrameworkEntry.GetModule<IObjectPoolManager>());
            m_UIManager.InstanceCapacity = m_InstanceCapacity;

            if (m_UIFormHelper == null)
            {
                m_UIFormHelper = (new GameObject()).AddComponent<DefaultUIFormHelper>();
                m_UIFormHelper.name = string.Format("UI Form Helper");
                Transform transform = m_UIFormHelper.transform;
                transform.SetParent(this.transform);
                transform.localScale = Vector3.one;
            }

            m_UIManager.SetUIFormHelper(m_UIFormHelper);

            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = (new GameObject("UI Instances")).transform;
                m_InstanceRoot.SetParent(gameObject.transform);
            }

            m_InstanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");

            foreach (UIGroup uiGroup in m_UIGroups)
            {
                if (!AddUIGroup(uiGroup.Name, uiGroup.Depth))
                {
                    Log.Warning("Add UI group '{0}' failed.", uiGroup.Name);
                    continue;
                }
            }
        }

        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            return m_UIManager.HasUIGroup(uiGroupName);
        }

        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        public IUIGroup GetUIGroup(string uiGroupName)
        {
            return m_UIManager.GetUIGroup(uiGroupName);
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        public IUIGroup[] GetAllUIGroups()
        {
            return m_UIManager.GetAllUIGroups();
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName)
        {
            return AddUIGroup(uiGroupName, 0);
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="depth">界面组深度。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int depth)
        {
            if (m_UIManager.HasUIGroup(uiGroupName))
            {
                return false;
            }

            UIGroupHelperBase helper = null;
            if (m_UIGroupHelperTemplate != null)
            {
                helper = Instantiate(m_UIGroupHelperTemplate);
            }
            else
            {
                helper = (new GameObject()).AddComponent<DefaultUIGroupHelper>();
            }

            helper.name = string.Format("UI Group - {0}", uiGroupName);
            helper.gameObject.layer = LayerMask.NameToLayer("UI");
            Transform transform = helper.transform;
            transform.SetParent(m_InstanceRoot);
            transform.localScale = Vector3.one;

            return m_UIManager.AddUIGroup(uiGroupName, depth, helper);
        }

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="uiFormTypeId">界面类型编号。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIForm(int uiFormTypeId, string uiGroupName)
        {
            return m_UIManager.HasUIForm(uiFormTypeId, uiGroupName);
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormTypeId">界面类型编号。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm GetUIForm(int uiFormTypeId, string uiGroupName)
        {
            return m_UIManager.GetUIForm(uiFormTypeId, uiGroupName) as UIForm;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormTypeId">界面类型编号。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm[] GetUIForms(int uiFormTypeId, string uiGroupName)
        {
            IUIForm[] uiForms = m_UIManager.GetUIForms(uiFormTypeId, uiGroupName);
            UIForm[] uiFormImpls = new UIForm[uiForms.Length];
            for (int i = 0; i < uiForms.Length; i++)
            {
                uiFormImpls[i] = uiForms[i] as UIForm;
            }

            return uiFormImpls;
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormTypeId">界面类型编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        public void OpenUIForm(int uiFormTypeId, string uiFormAssetName, string uiGroupName)
        {
            m_UIManager.OpenUIForm(uiFormTypeId, uiFormAssetName, uiGroupName);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormTypeId">界面类型编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        public void OpenUIForm(int uiFormTypeId, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
        {
            m_UIManager.OpenUIForm(uiFormTypeId, uiFormAssetName, uiGroupName, pauseCoveredUIForm);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormTypeId">界面类型编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OpenUIForm(int uiFormTypeId, string uiFormAssetName, string uiGroupName, object userData)
        {
            m_UIManager.OpenUIForm(uiFormTypeId, uiFormAssetName, uiGroupName, userData);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormTypeId">界面类型编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OpenUIForm(int uiFormTypeId, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData)
        {
            m_UIManager.OpenUIForm(uiFormTypeId, uiFormAssetName, uiGroupName, pauseCoveredUIForm, userData);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        public void CloseUIForm(UIForm uiForm)
        {
            m_UIManager.CloseUIForm(uiForm);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(UIForm uiForm, object userData)
        {
            m_UIManager.CloseUIForm(uiForm, userData);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        public void RefocusUIForm(UIForm uiForm)
        {
            m_UIManager.RefocusUIForm(uiForm);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(UIForm uiForm, object userData)
        {
            m_UIManager.RefocusUIForm(uiForm, userData);
        }

        /// <summary>
        /// 设置界面是否被加锁。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <param name="locked">界面是否被加锁。</param>
        public void SetUIFormLocked(UIForm uiForm, bool locked)
        {
            m_UIManager.SetUIFormLocked(uiForm, locked);
        }

        /// <summary>
        /// 设置界面的优先级。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <param name="priority">界面优先级。</param>
        public void SetUIFormPriority(UIForm uiForm, int priority)
        {
            m_UIManager.SetUIFormPriority(uiForm, priority);
        }

        private void OnOpenUIFormSuccess(object sender, GameFramework.UI.OpenUIFormSuccessEventArgs e)
        {
            m_EventComponent.Fire(this, new OpenUIFormSuccessEventArgs(e));
        }

        private void OnOpenUIFormFailure(object sender, GameFramework.UI.OpenUIFormFailureEventArgs e)
        {
            Log.Warning("Open UI form failure, asset name '{0}', UI group name '{1}', pause covered UI form '{2}', error message '{3}'.", e.UIFormAssetName, e.UIGroupName, e.PauseCoveredUIForm.ToString(), e.ErrorMessage);
            m_EventComponent.Fire(this, new OpenUIFormFailureEventArgs(e));
        }

        private void OnCloseUIFormComplete(object sender, GameFramework.UI.CloseUIFormCompleteEventArgs e)
        {
            m_EventComponent.Fire(this, new CloseUIFormCompleteEventArgs(e));
        }
    }
}
